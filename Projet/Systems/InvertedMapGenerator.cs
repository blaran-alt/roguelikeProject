using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using Projet.Core;
using RogueSharp.DiceNotation;

namespace Projet.Systems
{
    public class InvertedMapGenerator : MapGenerator
    {
        public InvertedMapGenerator(int width, int height) : base(width, height)
        {
            mapCenter = new Point(_width / 2, _height / 2);
            _map = new InvertedMap();
        }

        private Point mapCenter;

        public override GameMap CreateMap(int seed)
        {
            _map.Initialize(_width, _height);
            _map.SetCellProperties(mapCenter.X, mapCenter.Y, true, true);

            int maxDeltaX = _width / 2 - 3;
            int maxDeltaY = _height / 2 - 3;

            Point laserStart = Point.Zero;
            Cell addCell = (Cell)_map.GetCell(mapCenter.X, mapCenter.Y);

            bool yLimitReached = false;

            while (Math.Abs(addCell.X - mapCenter.X) < maxDeltaX)
            {
                List<ICell> line = GetStraightLine(laserStart, mapCenter);

                if (line.Last().IsWalkable && line.Count() > 1)
                {
                    addCell = (Cell)line[line.Count - 2];
                    _map.SetCellProperties(addCell.X, addCell.Y, true, true);
                    if (!yLimitReached && Math.Abs(addCell.Y - mapCenter.Y) >= maxDeltaY)
                    {
                        yLimitReached = true;
                    }
                }

                int x;
                int y;
                if (Game.Random.Next(5) > 3 || yLimitReached)
                {
                    x = Game.Random.Next(1) == 0 ? 0 : _width - 1;
                    y = Game.Random.Next(_height - 1);
                }
                else
                {
                    x = Game.Random.Next(_width - 1);
                    y = Game.Random.Next(1) == 0 ? 0 : _height - 1;
                }
                laserStart = new Point(x, y);
            }
            SetStartAndEndCell();
            ComputeDijkstraCellsFromStart();

            CreateStairs();

            PlacePlayer();
            PlaceBoxs();
            PlaceItems(_map.GetAllCells(),15, 3);
            PlaceMonsters(_map.GetAllCells().Except(_map.GetCellsInCircle(mapCenter.X, mapCenter.Y, 10)), 10, 5 );

            return _map;
        }

        private void SetStartAndEndCell()
        {
            _startCell = _map.GetCell(mapCenter.X, mapCenter.Y);
            Point endPos = _map.GetRandomWalkableLocationInRoom(_map.GetAllCells());
            _endCell = _map.GetCell(endPos.X, endPos.Y);
        }

        private void PlaceBoxs()
        {
            int[] maxs = new int[]{0,0,0,0};
            ICell[] dropCells = new ICell[4];
            foreach(ICell cell in DijkstraCells.Keys)
            {
                int dijkstraIndex = DijkstraCells[cell];
                int i = -1;
                if(cell.X <= mapCenter.X && cell.Y <= mapCenter.Y && dijkstraIndex > maxs[0])
                {
                    i = 0;
                }
                else if(cell.X > mapCenter.X && cell.Y <= mapCenter.Y && dijkstraIndex > maxs[1])
                {
                    i = 1;
                }
                else if (cell.X <= mapCenter.X && cell.Y > mapCenter.Y && dijkstraIndex > maxs[2])
                {
                    i = 2;
                }
                else if(cell.X > mapCenter.X && cell.Y > mapCenter.Y && dijkstraIndex > maxs[3])
                {
                    i = 3;
                }
                if(i != -1)
                {
                    maxs[i] = dijkstraIndex;
                    dropCells[i] = cell;
                }
            }
            int j = 0;
            foreach(ICell cell in dropCells)
            {
                Box box = new Box(cell.X, cell.Y, (j + 2) % 4);
                _map.AddBox(box);
                DropPad pad = new DropPad(cell.X, cell.Y, j);
                ((InvertedMap)_map).AddDropPad(pad);
                j++;
            }
        }
    }
}
