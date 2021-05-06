using System;
using RLNET;
using RogueSharp;
using Projet.Core;
using System.Linq;
using RogueSharp.DiceNotation;
using Projet.Monsters;
using System.Collections.Generic;
using Projet.Items;

namespace Projet.Systems
{
    public class MapGenerator
    {
        protected readonly int _width;
        protected readonly int _height;
        protected readonly int _maxRooms;
        protected readonly int _roomMaxSize;
        protected readonly int _roomMinSize;
        protected readonly string[] _existingItems;
        protected readonly int _mapLevel;

        protected GameMap _map;

        // Constructing a new MapGenerator requires the dimensions of the maps it will create
        public MapGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize, int mapLevel)
        {
            _width = width;
            _height = height;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _map = new GameMap();
            _existingItems = new string[] { "Potion", "Gold" };
            _mapLevel = mapLevel;
        }

        // Generate a new map that is a simple open floor with walls around the outside
        public virtual GameMap CreateMap(int seed)
        {
            // Initialize every cell in the map by
            // setting walkable, transparency, and explored to false
            _map.Initialize(_width, _height);

            for (int i = 0; i < _maxRooms; i++)
            {
                // Determine the size and position of the room randomly
                int roomWidth = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _width - roomWidth - 2);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 2);

                // All of our rooms can be represented as Rectangles
                var newRoom = new Rectangle(roomXPosition, roomYPosition,
                  roomWidth, roomHeight);

                // Check to see if the room rectangle intersects with any other rooms
                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                // As long as it doesn't intersect add it to the list of rooms
                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }

            for (int r = 1; r < _map.Rooms.Count; r++)
            {
                int previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[r].Center.X;
                int currentRoomCenterY = _map.Rooms[r].Center.Y;
                if (_mapLevel == 0)
                {
                    CreateTunnel(_map.Rooms[r-1], _map.Rooms[r]);
                }
                else
                {
                    if (Game.Random.Next(1, 2) == 0)
                    {
                        CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                        CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                        if (Game.Random.Next(1,2) == 1)
                        {
                            IEnumerable<Rectangle> connectRooms = _map.Rooms.Where(room => !room.Equals(_map.Rooms[r]) && !room.Equals(_map.Rooms[r - 1]));
                            if (connectRooms.Count() > 0)
                            {
                                Rectangle connectRoom = connectRooms.ElementAt(Game.Random.Next(0, connectRooms.Count() - 1));
                                int x = (previousRoomCenterX + currentRoomCenterX) / 2;
                                CreateVerticalTunnel(previousRoomCenterY, connectRoom.Center.Y, x);
                                CreateHorizontalTunnel(x, connectRoom.Center.X, connectRoom.Center.Y);
                            }
                        }
                    }
                    else
                    {
                        CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                        CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                        if (Game.Random.Next(1, 2) == 1)
                        {
                            IEnumerable<Rectangle> connectRooms = _map.Rooms.Where(room => !room.Equals(_map.Rooms[r]) && !room.Equals(_map.Rooms[r - 1]));
                            if (connectRooms.Count() > 0)
                            {
                                Rectangle connectRoom = connectRooms.ElementAt(Game.Random.Next(0, connectRooms.Count() - 1));
                                int y = (previousRoomCenterY + currentRoomCenterY) / 2;
                                CreateHorizontalTunnel(previousRoomCenterX, connectRoom.Center.X, y);
                                CreateVerticalTunnel(y, connectRoom.Center.Y, connectRoom.Center.X);
                            }
                        }
                    }
                }
            }

            foreach (Rectangle room in _map.Rooms)
            {
                CreateRoom(room);
                CreateDoors(room);
            }

            CreateStairs();

            PlacePlayer();
            PlaceMonsters();
            PlaceItems();

            Box box = new Box(_map.Rooms[0].Center.X + 1, _map.Rooms[0].Center.Y + 1);

            _map.AddBox(box);


            return _map;
        }

        private void CreateRoom(Rectangle room)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                for (int y = room.Top + 1; y < room.Bottom; y++)
                {
                    _map.SetCellProperties(x, y, true, true, false);
                }
            }
        }

        // Carve a tunnel out of the map parallel to the x-axis
        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetCellProperties(x, yPosition, true, true);
            }
        }

        // Carve a tunnel out of the map parallel to the y-axis
        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetCellProperties(xPosition, y, true, true);
            }
        }

        private void CreateTunnel(int xStart,int yStart, int xEnd, int yEnd)
        {
            Point distance = new Point(Math.Abs(xEnd - xStart), Math.Abs(yEnd - yStart));
            Point direction = new Point(Math.Sign(xEnd - xStart), Math.Sign(yEnd - yStart));
            int i = 0;
            while (i <= distance.X && i <= distance.Y)
            {
                _map.SetCellProperties(xStart + i * direction.X, yStart + i * direction.Y, true, true);
                if (distance.X >= distance.Y)
                {
                    _map.SetCellProperties(xStart + (i + 1) * direction.X, yStart + i * direction.Y, true, true);
                }
                else
                {
                    _map.SetCellProperties(xStart + i * direction.X, yStart + (i + 1) * direction.Y, true, true);
                }
                i++;
            }
            if (i < distance.X)
            {
                CreateHorizontalTunnel(xStart + i * direction.X, xEnd, yEnd);
            }
            else
            {
                CreateVerticalTunnel(yStart + i * direction.Y, yEnd, xEnd);
            }
        }

        private void CreateTunnel(Rectangle startRoom, Rectangle endRoom)
        {
            Point direction = new Point(Math.Sign(endRoom.Center.X - startRoom.Center.X), Math.Sign(endRoom.Center.Y - startRoom.Center.Y));
            Console.WriteLine(direction);
            Point distance = new Point(Math.Abs(endRoom.Center.X - startRoom.Center.X), Math.Abs(endRoom.Center.Y - startRoom.Center.Y));
            int xEnd;
            int xStart;
            int yEnd;
            int yStart;
            if(distance.X >= distance.Y)
            {
                xEnd = endRoom.Center.X - direction.X * endRoom.Width / 2;
                xStart = startRoom.Center.X + direction.X * startRoom.Width / 2;
                yEnd = endRoom.Center.Y;
                yStart = startRoom.Center.Y;
            }
            else
            {
                xEnd = endRoom.Center.X;
                xStart = startRoom.Center.X;
                yEnd = endRoom.Center.Y - direction.Y * endRoom.Height / 2;
                yStart = startRoom.Center.Y + direction.Y * startRoom.Height / 2;
            }
            distance = new Point(Math.Abs(xEnd - xStart),Math.Abs(yEnd - yStart));
            //int step = (int)Math.Floor((double)distance.Y/distance.X);
            //int remainder = distance.Y - step * (distance.X);
            //Console.WriteLine($"Room ({xStart},{yStart}) to room ({xEnd},{yEnd}) => step = {step} and remainder = {remainder}");
            int i = 0;
            bool connectTunnel = true;
            while(i <= distance.X && i <= distance.Y)
            {
                _map.SetCellProperties(xStart + i * direction.X, yStart + i * direction.Y, true, true);
                if(distance.X >= distance.Y)
                {
                    _map.SetCellProperties(xStart + (i + 1) * direction.X, yStart + i * direction.Y, true, true);
                }
                else
                {
                    _map.SetCellProperties(xStart + i * direction.X, yStart + (i + 1) * direction.Y, true, true);
                }
                if(connectTunnel && Game.Random.Next(1,3) == 1)
                {
                    IEnumerable<Rectangle> connectRooms = _map.Rooms.Where(room => !room.Equals(endRoom) && !room.Equals(startRoom));
                    if(connectRooms.Count() > 0)
                    {
                        Rectangle connectRoom = connectRooms.ElementAt(Game.Random.Next(0, connectRooms.Count() - 1));
                        CreateTunnel(xStart + i * direction.X, yStart + i * direction.Y, connectRoom.Center.X, connectRoom.Center.Y);
                    }
                    connectTunnel = false;
                }
                i++;
            }
            if(i < distance.X)
            {
                CreateHorizontalTunnel(xStart + i * direction.X, xEnd, yEnd);
            }
            else
            {
                CreateVerticalTunnel(yStart + i * direction.Y, yEnd, xEnd);
            }
        }

        private void PlacePlayer()
        {
            Player player = Game.Player;
            if (player == null)
            {
                player = new Player();
            }

            player.X = _map.Rooms[0].Center.X;
            player.Y = _map.Rooms[0].Center.Y;

            _map.AddPlayer(player);
        }

        private void PlaceMonsters()
        {
            foreach (var room in _map.Rooms)
            {
                // Each room has a 60% chance of having monsters
                if (Dice.Roll("1D10") < 7)
                {
                    // Generate between 1 and 4 monsters
                    var numberOfMonsters = Dice.Roll("1D4");
                    for (int i = 0; i < numberOfMonsters; i++)
                    {
                        // Find a random walkable location in the room to place the monster
                        Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(room);
                        // It's possible that the room doesn't have space to place a monster
                        // In that case skip creating the monster
                        if (randomRoomLocation != Point.Zero)
                        {
                            // Create a monster
                            var monster = Coupeur.Create(Game.Level);
                            monster.Coord = randomRoomLocation;
                            _map.AddMonster(monster);
                        }
                    }
                }
            }
        }

        private void PlaceItems()
        {
            int keyPlaced = 1;
            int roomIndex = 0;
            int roomCount = _map.Rooms.Count();
            int previousCheckpoint = 0;
            foreach (var room in _map.Rooms)
            {
                // Making sure that the 3 keys are placed
                if(roomIndex > 0 && roomIndex < roomCount - 1 && keyPlaced < 4)
                {
                    int checkpoint = keyPlaced * (roomCount - 2) / 3;
                    Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(room);
                    if (randomRoomLocation != Point.Zero)
                    {
                        if (roomIndex >= checkpoint)
                        {
                            Item key = new Key(keyPlaced - 1, randomRoomLocation.X, randomRoomLocation.Y);
                            _map.AddItem(key);
                            keyPlaced++;
                            previousCheckpoint = checkpoint;
                        }
                        else
                        {
                            int diceNb = (roomIndex <= previousCheckpoint) ? 1 : roomIndex - previousCheckpoint;
                            DiceExpression dice = new DiceExpression()
                                .Dice(diceNb, 2*(checkpoint - previousCheckpoint - 1));
                            if (dice.Roll().Value > 8)
                            {
                                Item key = new Key(keyPlaced - 1, randomRoomLocation.X, randomRoomLocation.Y);
                                _map.AddItem(key);
                                keyPlaced++;
                                previousCheckpoint = checkpoint;
                            }
                        }
                    }
                }
                // Placing the other items
                if (Dice.Roll("1D10") < 7)
                {
                    // Generate between 1 and 4 items
                    var numberOfItems = Dice.Roll("1D3");
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        string itemName = _existingItems[Game.Random.Next(0, _existingItems.Length - 1)];
                        Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(room);
                        if (randomRoomLocation != Point.Zero)
                        {
                            Item item;
                            if (itemName == "Gold")
                            {
                                int quantity = Dice.Roll("3D5");
                                item = new Gold(quantity, randomRoomLocation.X, randomRoomLocation.Y);
                            }
                            else
                            {
                                int effectCode = Game.Random.Next(0, 2);
                                item = new Potion(effectCode, randomRoomLocation.X, randomRoomLocation.Y);
                            }
                            _map.AddItem(item);
                        }
                    }
                }
                roomIndex++;
            }
        }

        private void CreateDoors(Rectangle room)
        {
            // The the boundries of the room
            int xMin = room.Left;
            int xMax = room.Right;
            int yMin = room.Top;
            int yMax = room.Bottom;

            // Put the rooms border cells into a list
            List<ICell> borderCells = _map.GetCellsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMax, yMin, xMax, yMax));

            // Go through each of the rooms border cells and look for locations to place doors.
            foreach (ICell cell in borderCells)
            {
                if (IsPotentialDoor(cell))
                {
                    // A door must block field-of-view when it is closed.
                    _map.SetCellProperties(cell.X, cell.Y, false, true);
                    _map.Doors.Add(new Door(cell.X, cell.Y));
                }
            }
        }

        // Checks to see if a cell is a good candidate for placement of a door
        protected bool IsPotentialDoor(ICell cell)
        {
            // If the cell is not walkable
            // then it is a wall and not a good place for a door
            if (!cell.IsWalkable)
            {
                return false;
            }

            // Store references to all of the neighboring cells
            int x = cell.X;
            int y = cell.Y;

            ICell right;
            ICell left;
            ICell top;
            ICell bottom;

            if (_map.IsInMap(x + 1, y))
            {
                right = _map.GetCell(x + 1, y);
            }
            else
            {
                right = new Cell(x + 1, y, false, false, false);
            }
            if (_map.IsInMap(x - 1, y))
            {
                left = _map.GetCell(x - 1, y);
            }
            else
            {
                left = new Cell(x - 1, y, false, false, false);
            }
            if (_map.IsInMap(x, y - 1))
            {
                top = _map.GetCell(x, y - 1);
            }
            else
            {
                top = new Cell(x, y - 1, false, false, false);
            }
            if (_map.IsInMap(x, y + 1))
            {
                bottom = _map.GetCell(x, y + 1);
            }
            else
            {
                bottom = new Cell(x, y + 1, false, false, false);
            }

            // Make sure there is not already a door here
            if (_map.GetDoor(cell.X, cell.Y) != null ||
                _map.GetDoor(right.X, right.Y) != null ||
                _map.GetDoor(left.X, left.Y) != null ||
                _map.GetDoor(top.X, top.Y) != null ||
                _map.GetDoor(bottom.X, bottom.Y) != null)
            {
                return false;
            }

            // This is a good place for a door on the left or right side of the room
            if (right.IsWalkable && left.IsWalkable && !top.IsWalkable && !bottom.IsWalkable)
            {
                return true;
            }

            // This is a good place for a door on the top or bottom of the room
            if (!right.IsWalkable && !left.IsWalkable && top.IsWalkable && bottom.IsWalkable)
            {
                return true;
            }
            return false;
        }

        private void CreateStairs()
        {
            _map.StairsUp = new Stairs(_map.Rooms.First().Center.X + 1, _map.Rooms.First().Center.Y, true);
            _map.StairsDown = new Stairs(_map.Rooms.Last().Center.X, _map.Rooms.Last().Center.Y, false);
        }
    }
}
