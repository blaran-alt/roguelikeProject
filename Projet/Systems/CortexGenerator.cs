using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RLNET;
using RogueSharp;

namespace Projet.Systems
{
    public class CortexGenerator : MapGenerator
    {

        public CortexGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize, int mapLevel) : base(width, height, maxRooms, roomMaxSize, roomMinSize, mapLevel)
        {}

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

            _map.dijkstraIndeces = new int[_width, _height];
            PlacePlayer();

            return _map;
        }

        private void PlacePlayer()
        {
            Player player = Game.Player;
            if (player == null)
            {
                player = new Player();
            }
            ICell startCell;
            do
            {
                //Chose a walkable starting cell
                do
                {
                    startCell = _map.GetCell(Game.Random.Next(0, _width - 1), Game.Random.Next(0, _height - 1));
                } while (!startCell.IsWalkable);
                //Compute the dijkstra cells to obtain a map with at least a 170 cells walkable distance
            } while (ComputeDijkstraIndeces(startCell) < 170);

            //Place the player on the chosen cell
            player.X = startCell.X;
            player.Y = startCell.Y;
            _map.AddPlayer(player);
        }

        //Calculate distance to start for every cells and return the max distance
        private int ComputeDijkstraIndeces(ICell startCell)
        {
            //On remet toutes les valeurs du tableau à -1
            _map.ResetDijkstraIndeces();

            Queue<ICell> dijkstraCells = new Queue<ICell>();
            _map.dijkstraIndeces[startCell.X, startCell.Y] = 0;
            dijkstraCells.Enqueue(startCell);
            int maxDistance = 0;
            while(dijkstraCells.Count > 0)
            {
                ICell cell = dijkstraCells.Dequeue();
                int minCloseIndex = int.MaxValue;
                bool updateValue = false;
                foreach (ICell closeCell in _map.GetBorderCellsInCircle(cell.X, cell.Y, 1))
                {
                    if (closeCell.IsWalkable)
                    {
                        int distance = _map.dijkstraIndeces[closeCell.X, closeCell.Y];
                        if(distance == -1)
                        {
                            if (!dijkstraCells.Contains(closeCell))
                            {
                                dijkstraCells.Enqueue(closeCell);
                            }
                        }
                        else if (distance < minCloseIndex)
                        {
                            minCloseIndex = distance;
                            updateValue = true;
                        }
                    }
                }
                if (updateValue)
                {
                    _map.dijkstraIndeces[cell.X, cell.Y] = minCloseIndex + 1;
                    if(minCloseIndex + 1 > maxDistance)
                    {
                        maxDistance = minCloseIndex + 1;
                    }
                }
            }
            return maxDistance;
        }
    }
}
