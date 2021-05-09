using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RLNET;
using RogueSharp;
using Projet.Monsters;
using RogueSharp.DiceNotation;
using Projet.Items;

namespace Projet.Systems
{
    public class NeuronMapGenerator : MapGenerator
    {
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;

        public NeuronMapGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize) : base(width, height)
        {
            _map = new NeuronMap();
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
        }

        public override GameMap CreateMap(int seed)
        {
            base.CreateMap(seed);

            for (int i = 0; i < _maxRooms; i++)
            {
                // Determine the size and position of the room randomly
                int roomWidth = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _width - roomWidth - 2);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 2);

                // All of our rooms can be represented as Rectangles
                Rectangle rectangle = new Rectangle(roomXPosition, roomYPosition, roomWidth, roomHeight);
                Room newRoom = new Room(_map, rectangle);

                // Check to see if the room rectangle intersects with any other rooms
                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.BaseRectangle.Intersects(room.BaseRectangle));

                // As long as it doesn't intersect add it to the list of rooms
                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }

            for (int r = 1; r < _map.Rooms.Count; r++)
            {
                int previousRoomCenterX = _map.Rooms[r - 1].BaseRectangle.Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].BaseRectangle.Center.Y;
                int currentRoomCenterX = _map.Rooms[r].BaseRectangle.Center.X;
                int currentRoomCenterY = _map.Rooms[r].BaseRectangle.Center.Y;
                if (true)
                {
                    CreateTunnel(_map.Rooms[r - 1].BaseRectangle, _map.Rooms[r].BaseRectangle);
                }
                else
                {
                    if (Game.Random.Next(1, 2) == 0)
                    {
                        CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                        CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                        if (Game.Random.Next(1, 2) == 1)
                        {
                            List<Rectangle> connectRooms = _map.Rooms.Where(room => !room.Equals(_map.Rooms[r]) && !room.Equals(_map.Rooms[r - 1])).ToList().ConvertAll(c => c.BaseRectangle);
                            if (connectRooms.Count() > 0)
                            {
                                Rectangle connectRoom = connectRooms[Game.Random.Next(0, connectRooms.Count() - 1)];
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
                            List<Rectangle> connectRooms = _map.Rooms.Where(room => !room.Equals(_map.Rooms[r]) && !room.Equals(_map.Rooms[r - 1])).ToList().ConvertAll(c => c.BaseRectangle);
                            if (connectRooms.Count() > 0)
                            {
                                Rectangle connectRoom = connectRooms[Game.Random.Next(0, connectRooms.Count() - 1)];
                                int y = (previousRoomCenterY + currentRoomCenterY) / 2;
                                CreateHorizontalTunnel(previousRoomCenterX, connectRoom.Center.X, y);
                                CreateVerticalTunnel(y, connectRoom.Center.Y, connectRoom.Center.X);
                            }
                        }
                    }
                }
            }

            _startCell = _map.GetCell(_map.Rooms.First().BaseRectangle.Center.X + 1, _map.Rooms.First().BaseRectangle.Center.Y);
            _endCell = _map.GetCell(_map.Rooms.Last().BaseRectangle.Center.X, _map.Rooms.Last().BaseRectangle.Center.Y);

            for (int i = 0; i < 2000; i++)
            {
                Point laserStart = new Point(Game.Random.Next(_width - 1), Game.Random.Next(_height - 1));
                if(_map.GetCell(laserStart.X, laserStart.Y).IsWalkable)
                {
                    continue;
                }
                Point laserDir;
                do
                {
                    laserDir = new Point(Game.Random.Next(_width - 1), Game.Random.Next(_height - 1));
                } while (laserDir == laserStart);

                //laserStart = new Point(20, 10);
                //laserDir = new Point(0, 0);

                List<ICell> line = GetStraightLine(laserStart, laserDir);
                ICell hitcell;
                if(line != null && (hitcell = line.Last()).IsWalkable)
                {
                    int index = line.Count - 1;
                    if (index >= 0)
                    {
                        Cell addCell = (Cell)line[index - 1];
                        Room room = _map.GetRoom((Cell)hitcell);
                        if (!room.Equals(Room.Empty))
                        {
                            room.AddCell(addCell);
                        }
                        _map.SetCellProperties(addCell.X, addCell.Y, true, true);
                    }
                }
            }

            foreach(Room room in _map.Rooms)
            {
                CreateRoom(room);
            }

            CreateStairs();

            PlacePlayer();
            PlaceMonsters();
            PlaceItems();

            return _map;
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

        private void CreateTunnel(int xStart, int yStart, int xEnd, int yEnd)
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
            Map emptyMap = new Map(_width, _height);
            foreach (ICell _cell in emptyMap.GetAllCells())
            {
                emptyMap.SetCellProperties(_cell.X, _cell.Y, true, true);
            }
            PathFinder pathFinder = new PathFinder(emptyMap);
            Path path = pathFinder.ShortestPath(emptyMap.GetCell(startRoom.Center.X, startRoom.Center.Y), emptyMap.GetCell(endRoom.Center.X, endRoom.Center.Y));
            ICell cell = path.CurrentStep;
            while (cell != null)
            {
                _map.SetCellProperties(cell.X, cell.Y, true, true);
                cell = path.TryStepForward();
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
                if (roomIndex > 0 && roomIndex < roomCount - 1 && keyPlaced < 4)
                {
                    int checkpoint = keyPlaced * (roomCount - 2) / 3;
                    Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(room);
                    if (randomRoomLocation != Point.Zero)
                    {
                        if (roomIndex >= checkpoint)
                        {
                            Item key = new Key(randomRoomLocation.X, randomRoomLocation.Y, keyPlaced - 1);
                            _map.AddItem(key);
                            keyPlaced++;
                            previousCheckpoint = checkpoint;
                        }
                        else
                        {
                            int diceNb = (roomIndex <= previousCheckpoint) ? 1 : roomIndex - previousCheckpoint;
                            DiceExpression dice = new DiceExpression()
                                .Dice(diceNb, 2 * (checkpoint - previousCheckpoint - 1));
                            if (dice.Roll().Value > 8)
                            {
                                Item key = new Key(randomRoomLocation.X, randomRoomLocation.Y, keyPlaced - 1);
                                _map.AddItem(key);
                                keyPlaced++;
                                previousCheckpoint = checkpoint;
                            }
                        }
                    }
                }
                // Placer les items restants
                if (Dice.Roll("1D10") < 8)
                {
                    PlaceItems(room.GetAllCells(_map), 3);
                }
                roomIndex++;
            }
        }


        private void PlaceMonsters()
        {
            foreach (var room in _map.Rooms)
            {
                // On a 60% de chance d'avoir des monstres dans une salle
                if (Dice.Roll("1D10") < 7)
                {
                    PlaceMonsters(room.GetAllCells(_map), 4);
                }
            }
        }
    }
}
