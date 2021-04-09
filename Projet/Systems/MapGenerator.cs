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
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;
        private readonly string[] _existingItems;

        private readonly GameMap _map;

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
        }

        // Generate a new map that is a simple open floor with walls around the outside
        public GameMap CreateMap()
        {
            // Initialize every cell in the map by
            // setting walkable, transparency, and explored to true
            _map.Initialize(_width, _height);

            for (int i = 0; i < _maxRooms; i++)
            {
                // Determine the size and position of the room randomly
                int roomWidth = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _width - roomWidth - 1);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 1);

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

                if (Game.Random.Next(1, 2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
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
                            // Temporarily hard code this monster to be created at level 1
                            var monster = Kobold.Create(Game.Level);
                            monster.X = randomRoomLocation.X;
                            monster.Y = randomRoomLocation.Y;
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
                    _map.Doors.Add(new Door
                    {
                        X = cell.X,
                        Y = cell.Y,
                        IsOpen = false
                    });
                }
            }
        }

        // Checks to see if a cell is a good candidate for placement of a door
        private bool IsPotentialDoor(ICell cell)
        {
            // If the cell is not walkable
            // then it is a wall and not a good place for a door
            if (!cell.IsWalkable)
            {
                return false;
            }

            // Store references to all of the neighboring cells 
            ICell right = _map.GetCell(cell.X + 1, cell.Y);
            ICell left = _map.GetCell(cell.X - 1, cell.Y);
            ICell top = _map.GetCell(cell.X, cell.Y - 1);
            ICell bottom = _map.GetCell(cell.X, cell.Y + 1);

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
            _map.StairsUp = new Stairs
            {
                X = _map.Rooms.First().Center.X + 1,
                Y = _map.Rooms.First().Center.Y,
                IsUp = true
            };
            _map.StairsDown = new Stairs
            {
                X = _map.Rooms.Last().Center.X,
                Y = _map.Rooms.Last().Center.Y,
                IsUp = false
            };
        }
    }
}
