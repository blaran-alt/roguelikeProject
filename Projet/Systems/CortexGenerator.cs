using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RLNET;
using RogueSharp;
using RogueSharp.DiceNotation;
using Projet.Monsters;

namespace Projet.Systems
{
    public class CortexGenerator : MapGenerator
    {
        public Dictionary<ICell, int> DijkstraCells;
        public Dictionary<ICell, int> PathDijkstraCells;
        public List<ICell> HotPath;
        private Queue<ICell> toProcessCells;

        public Path startToEndPath;
        private int hotPathWidth = 15;
        
        private ICell _startCell;
        private int _maxDistance;

        public CortexGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize, int mapLevel) : base(width, height, maxRooms, roomMaxSize, roomMinSize, mapLevel)
        {
        }

        public override GameMap CreateMap(int seed)
        {
            _map = new CortexMap();
            _map.Initialize(_width,_height);
            float[,] noiseMap = PerlinNoise.GeneratePerlinNoiseMap(_width, _height, seed);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (noiseMap[x, y] > 90)
                    {
                        _map.SetCellProperties(x, y, true, true, false);
                    }
                }
            }

            DijkstraCells = new Dictionary<ICell, int>();
            PathDijkstraCells = new Dictionary<ICell, int>();
            toProcessCells = new Queue<ICell>();

            SetStartCell();

            List<ICell> firstRoom = _map.GetCellsInCircle(_startCell.X, _startCell.Y, 7).Where(c => DijkstraCells.ContainsKey(c) && DijkstraCells[c] >= 3 && DijkstraCells[c] >= 7).ToList();
            ICell furthestCell = DijkstraCells.Last().Key;
            PathFinder pathFinder = new PathFinder(_map);
            startToEndPath = pathFinder.TryFindShortestPath(_startCell, furthestCell);
            ComputeHotPath(startToEndPath);
            
            //-------------------------------
            if(startToEndPath == null)
            {
                Console.WriteLine("Erreur");
            }

            List<ICell> surroundingCells = _map.GetCellsInCircle(_startCell.X, _startCell.Y, 1).Where(c => DijkstraCells.ContainsKey(c) && DijkstraCells[c] == 1).ToList();
            ICell playerStartCell = surroundingCells[Game.Random.Next(surroundingCells.Count - 1)];
            PlacePlayer(playerStartCell);

            CreateStairs(furthestCell);
            PlaceItems(firstRoom);
            PlaceTerminals();
            PlaceMonsters();


            return _map;
        }

        private void CreateStairs(ICell lastCell)
        {
            _map.StairsUp = new Stairs(_startCell.X, _startCell.Y, true);
            _map.StairsDown = new Stairs(lastCell.X, lastCell.Y, false);
        }

        private void PlaceTerminals()
        {
            for(int i = 0; i < _map.nbConnections; i++)
            {
                ICell cell1;
                ICell cell2;
                List<ICell> hotPathRest = HotPath.Where(c => DijkstraCells[c] > 20).ToList(); 
                List<ICell> coldPath = PathDijkstraCells.Where(c => c.Value > hotPathWidth && DijkstraCells[c.Key] > 20).Select(c => c.Key).ToList();
                do
                {
                    // One termial will be in the center of the map
                    cell1 = hotPathRest[Game.Random.Next(hotPathRest.Count() - 1)];
                    // The other one will be more on the borders
                    cell2 = coldPath[Game.Random.Next(coldPath.Count() - 1)];

                    //If it's a potential door it might block the way to a room or a side of the map
                } while (MayBlockWay(cell1) || MayBlockWay(cell2));

                // Once we have the right spawn cells, we place the terminals
                Terminal terminal1 = new Terminal(cell1.X, cell1.Y, Colors.TerminalColors[i]);
                Terminal terminal2 = new Terminal(cell2.X, cell2.Y, Colors.TerminalColors[i]);
                _map.AddTerminals(new Connection(terminal1, terminal2));
            }
        }


        private bool MayBlockWay(ICell place)
        {
            foreach(ICell cell in _map.GetCellsInCircle(place.X, place.Y, 1))
            {
                if (IsPotentialDoor(cell))
                {
                    return true;
                }
            }
            return false;
        }

        private void PlaceMonsters()
        {
            foreach (Connection connection in _map.Connections)
            {
                List<ICell> spawnZone = _map.GetCellsInCircle(connection.TerminalA.X, connection.TerminalA.Y, 6).Where(c => c.IsWalkable).ToList();
                for (int i = 0; i < Dice.Roll("2D3-2"); i++)
                {
                    ICell spawnCell = spawnZone[Game.Random.Next(spawnZone.Count - 1)];
                    var cutter = Coupeur.Create(Game.Level);
                    cutter.X = spawnCell.X;
                    cutter.Y = spawnCell.Y;
                    _map.AddMonster(cutter);
                }
                spawnZone = _map.GetCellsInCircle(connection.TerminalB.X, connection.TerminalB.Y, 6).Where(c => c.IsWalkable).ToList();
                for (int i = 0; i < Dice.Roll("2D3"); i++)
                {
                    ICell spawnCell = spawnZone[Game.Random.Next(spawnZone.Count - 1)];
                    var cutter = Coupeur.Create(Game.Level);
                    cutter.X = spawnCell.X;
                    cutter.Y = spawnCell.Y;
                    _map.AddMonster(cutter);
                }
            }
        }

        private void PlaceItems(List<ICell> firstRoom)
        {
            //ICell firstItemCell = firstRoom[Game.Random.Next(firstRoom.Count - 1)];
        }

        private void SetStartCell()
        {
            int maxDistance;
            do
            {
                //Chose a walkable starting cell
                do
                {
                    _startCell = _map.GetCell(Game.Random.Next(0, _width - 1), Game.Random.Next(0, _height - 1));
                } while (!_startCell.IsWalkable);
                //Compute the dijkstra cells to obtain a map with at least a 170 cells walkable distance
                maxDistance = ComputeDijkstraCellsFromStart();
            } while (maxDistance < 170);
            _maxDistance = maxDistance;
        }

        private void PlacePlayer(ICell spawnCell)
        {
            Player player = Game.Player;
            if (player == null)
            {
                player = new Player();
            }

            //Place the player on the chosen cell
            player.X = spawnCell.X;
            player.Y = spawnCell.Y;
            _map.AddPlayer(player);
        }

        private int ComputeDijkstraCellsFromStart()
        {
            //On reset le dictionnaire des valeurs
            DijkstraCells.Clear();
            toProcessCells.Clear();
            return ComputeDijkstraIndeces(_startCell, DijkstraCells);
        }

        //Calculate distance to startCell for every cells and return the max
        private int ComputeDijkstraIndeces(ICell startCell, Dictionary<ICell, int> dict)
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
                        if(!dict.ContainsKey(closeCell))
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

        private void ComputeHotPath(Path hotPath)
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

        //private List<ICell> GetWalkableCellsAroundPoint(Point center, int radius)
        //{
        //    return _map.GetCellsInCircle(center.X,center.Y,radius).Where(c => DijkstraCells.ContainsKey(c) && DijkstraCells[c] <= radius).ToList();
        //}
        //private List<ICell> GetWalkableCellsAroundPoint(ICell center, int radius)
        //{
        //    return _map.GetCellsInCircle(center.X, center.Y, radius).Where(c => DijkstraCells.ContainsKey(c) && DijkstraCells[c] <= radius).ToList();
        //}
        //private List<ICell> GetWalkableCellsAroundPoint(Point center, int innerRadius, int outterRadius)
        //{
        //    return GetWalkableCellsAroundPoint(center, outterRadius).Where(c => DijkstraCells[c] >= innerRadius).ToList();
        //}
        //private List<ICell> GetWalkableCellsAroundPoint(ICell center, int innerRadius, int outterRadius)
        //{
        //    return GetWalkableCellsAroundPoint(center, outterRadius).Where(c => DijkstraCells[c] >= innerRadius).ToList();
        //}
    }
}
