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
    public class CortexMapGenerator : MapGenerator
    {
        public CortexMapGenerator(int width, int height) : base(width, height)
        {
            _map = new CortexMap();
        }

        public override GameMap CreateMap(int seed)
        {
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

            SetStartAndEndCell();

            List<ICell> firstRoom = _map.GetCellsInCircle(_startCell.X, _startCell.Y, 7).Where(c => DijkstraCells.ContainsKey(c) && DijkstraCells[c] >= 3 && DijkstraCells[c] >= 7).ToList();
            ICell furthestCell = DijkstraCells.Last().Key;
            PathFinder pathFinder = new PathFinder(_map);
            startToEndPath = pathFinder.TryFindShortestPath(_startCell, furthestCell);
            ComputeHotPath(startToEndPath);

            List<ICell> surroundingCells = _map.GetCellsInCircle(_startCell.X, _startCell.Y, 1).Where(c => DijkstraCells.ContainsKey(c) && DijkstraCells[c] == 1).ToList();
            ICell playerStartCell = surroundingCells[Game.Random.Next(surroundingCells.Count - 1)];
            PlacePlayer(playerStartCell);

            CreateStairs(furthestCell);
            PlaceItems(firstRoom, 3, 2);
            PlaceItems(HotPath, 5, 7);
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

                cell1 = hotPathRest[Game.Random.Next(hotPathRest.Count() - 1)];
                cell2 = coldPath[Game.Random.Next(coldPath.Count() - 1)];

                // Once we have the right spawn cells, we place the terminals
                Terminal terminal1 = new Terminal(cell1.X, cell1.Y, Colors.TerminalColors[i]);
                Terminal terminal2 = new Terminal(cell2.X, cell2.Y, Colors.TerminalColors[i]);
                _map.AddTerminals(new Connection(terminal1, terminal2));
            }
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

        private void SetStartAndEndCell()
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
    }
}
