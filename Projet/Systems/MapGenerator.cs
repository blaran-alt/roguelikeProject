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
    public abstract class MapGenerator
    {
        protected readonly int _width;
        protected readonly int _height;
        protected readonly string[] _existingItems;
        protected readonly string[] _existingMonsters;

        protected GameMap _map;

        // Constructing a new MapGenerator requires the dimensions of the maps it will create
        public MapGenerator(int width, int height)
        {
            _width = width;
            _height = height;
            _existingItems = new string[] { "Potion", "Gold" };
            _existingMonsters = new string[] { "Coupeur", "Bodybuilder", "Coquard" };
            DijkstraCells = new Dictionary<ICell, int>();
            PathDijkstraCells = new Dictionary<ICell, int>();
            toProcessCells = new Queue<ICell>();
        }

        // Generate a new map that is a simple open floor with walls around the outside
        public virtual GameMap CreateMap(int seed)
        {
            _map.Initialize(_width, _height);
            return _map;
        }

        protected void CreateRoom(Rectangle room)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                for (int y = room.Top + 1; y < room.Bottom; y++)
                {
                    _map.SetCellProperties(x, y, true, true, false);
                }
            }
        }
        protected void CreateRoom(Room room)
        {
            CreateRoom(room.BaseRectangle);
            foreach(Cell cell in room.Cells)
            {
                _map.SetCellProperties(cell.X, cell.Y, true, true, false);
            }
        }

        protected void PlaceMonsters(IEnumerable<ICell> spawZone, int maxMonsterNb)
        {
            PlaceMonsters(spawZone, 1, maxMonsterNb);
        }
        protected void PlaceMonsters(IEnumerable<ICell> spawZone, int minMonsterNb, int maxMonsterNb)
        {
            // On place entre 1 et 4 monstres
            var numberOfMonsters = Dice.Roll(minMonsterNb.ToString() + "D" + maxMonsterNb.ToString());
            for (int i = 0; i < numberOfMonsters; i++)
            {
                string monsterName = _existingMonsters[Dice.Roll("1D" + Game.Level.ToString()) - 1];
                // Find a random walkable location in the room to place the monster
                Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(spawZone);
                // It's possible that the room doesn't have space to place a monster
                // In that case skip creating the monster
                if (randomRoomLocation != Point.Zero)
                {
                    Monster monster;
                    switch (monsterName)
                    {
                        case "Coupeur":
                            monster = Coupeur.Create(Game.Level);
                            break;
                        case "Bodybuilder":
                            monster = Bodybuilder.Create(Game.Level);
                            break;
                        default:
                            monster = Coquard.Create(Game.Level);
                            break;
                    }
                    monster.Coord = randomRoomLocation;
                    _map.AddMonster(monster);
                }
            }
        }

        protected void PlaceItems(IEnumerable<ICell> spawZone, int maxItemNb)
        {
            PlaceItems(spawZone, 1, maxItemNb);
        }
        protected void PlaceItems(IEnumerable<ICell> spawZone, int minItemNb, int maxItemNb)
        {
            // Genere entre 1 et 3 items;
            var numberOfItems = Dice.Roll(minItemNb.ToString() + "D" + maxItemNb.ToString());
            for (int i = 0; i < numberOfItems; i++)
            {
                string itemName = _existingItems[Dice.Roll("1D" + (_existingItems.Length).ToString()) - 1];
                Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(spawZone);
                if (randomRoomLocation != Point.Zero)
                {
                    int effectCode = Game.Random.Next(0, 2);
                    Item item = new Potion(randomRoomLocation.X, randomRoomLocation.Y, effectCode);
                    _map.AddItem(item);
                }
            }
        }

        protected Dictionary<ICell, int> DijkstraCells;
        protected Dictionary<ICell, int> PathDijkstraCells;
        protected List<ICell> HotPath;
        protected Queue<ICell> toProcessCells;

        public Path startToEndPath;
        protected int hotPathWidth = 15;

        protected ICell _startCell;
        protected ICell _endCell;

        protected int ComputeDijkstraCellsFromStart()
        {
            //On reset le dictionnaire des valeurs
            DijkstraCells.Clear();
            toProcessCells.Clear();
            return ComputeDijkstraIndeces(_startCell, DijkstraCells);
        }

        //Calculate distance to startCell for every cells and return the max
        protected int ComputeDijkstraIndeces(ICell startCell, Dictionary<ICell, int> dict)
        {
            dict[startCell] = 0;
            toProcessCells.Enqueue(startCell);
            int maxDistance = 0;
            while (toProcessCells.Count > 0)
            {
                ICell cell = toProcessCells.Dequeue();
                int minCloseIndex;
                if (dict.ContainsKey(cell))
                {
                    minCloseIndex = dict[cell];
                }
                else
                {
                    minCloseIndex = int.MaxValue;
                }
                bool updateValue = false;
                foreach (ICell closeCell in _map.GetBorderCellsInCircle(cell.X, cell.Y, 1))
                {
                    if (closeCell.IsWalkable)
                    {
                        if (!dict.ContainsKey(closeCell))
                        {
                            if (!toProcessCells.Contains(closeCell))
                            {
                                toProcessCells.Enqueue(closeCell);
                            }
                        }
                        else if (dict[closeCell] + 1 < minCloseIndex)
                        {
                            minCloseIndex = dict[closeCell] + 1;
                            updateValue = true;
                        }
                    }
                }
                if (updateValue && !dict.ContainsKey(cell))
                {
                    dict[cell] = minCloseIndex;
                    if (minCloseIndex + 1 > maxDistance)
                    {
                        maxDistance = minCloseIndex + 1;
                    }
                }
            }
            return maxDistance;
        }

        protected void ComputeHotPath(Path hotPath)
        {
            toProcessCells.Clear();

            foreach (ICell cell in hotPath.Steps)
            {
                PathDijkstraCells[cell] = 0;
                toProcessCells.Enqueue(cell);
            }
            ComputeDijkstraIndeces(_startCell, PathDijkstraCells);

            HotPath = PathDijkstraCells.Where(c => c.Value <= hotPathWidth).Select(c => c.Key).ToList();
        }

        protected List<ICell> GetStraightLine(Point start, Point end)
        {
            List<ICell> line = new List<ICell>();
            int deltaX = end.X - start.X;
            int deltaY = end.Y - start.Y;
            float gradient = deltaY / (float)deltaX;
            Point dir = new Point(Math.Sign(deltaX), Math.Sign(deltaY));

            if (deltaX == 0)
            {
                for (int i = 0; i <= Math.Abs(deltaY); i++)
                {
                    Cell mapCell = (Cell)_map.GetCell(start.X, start.Y + dir.Y * i);
                    line.Add(mapCell);
                    if (mapCell.IsWalkable)
                    {
                        return line;
                    }
                }
                return line;
            }

            if (dir.X < 0)
            {
                gradient = -gradient;
            }
            Point last = start;

            for (int i = 0; i <= Math.Abs(deltaX); i++)
            {
                int x = start.X + i * dir.X;
                int y = Game.Clamp(start.Y + (int)Math.Round(i * gradient), 0, _height - 1);
                Cell mapCell;
                bool isDiag = last.Y != y;
                if (isDiag)
                {
                    Cell adjacentCell = (Cell)_map.GetCell(last.X, last.Y + dir.Y);
                    line.Add(adjacentCell);
                    if (adjacentCell.IsWalkable)
                    {
                        return line;
                    }
                }
                while (isDiag && y != last.Y + dir.Y)
                {
                    last.Y += dir.Y;
                    mapCell = (Cell)_map.GetCell(x, last.Y);
                    line.Add(mapCell);
                    if (mapCell.IsWalkable)
                    {
                        return line;
                    }
                }
                mapCell = (Cell)_map.GetCell(x, y);
                line.Add(mapCell);
                if (mapCell.IsWalkable)
                {
                    return line;
                }
                last = new Point(x, y);
            }
            return line;
        }

        protected void PlacePlayer()
        {
            Player player = Game.Player;
            if (player == null)
            {
                player = new Player();
            }

            player.X = _startCell.X;
            player.Y = _startCell.Y;

            _map.AddPlayer(player);
        }

        protected virtual void CreateStairs()
        {
            _map.StairsUp = new Stairs(_startCell.X + 1, _startCell.Y, true);
            _map.StairsDown = new Stairs(_endCell.X, _endCell.Y, false);
        }
    }
}
