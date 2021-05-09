using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using Projet.Core;

namespace Projet.Systems
{
    public struct Room
    {
        public static Room Empty { get; }

        public List<ICell> GetAllCells(GameMap map)
        {
            List<ICell> rectangelCells = new List<ICell>();
            for (int i = BaseRectangle.Left; i <= BaseRectangle.Right; i++)
            {
                for (int j = BaseRectangle.Top; j <= BaseRectangle.Bottom; j++)
                {
                    rectangelCells.Add(map.GetCell(i, j));
                }
            }
            return rectangelCells.Concat(_cells).ToList();
        }

        private List<Cell> _cells;
        public Rectangle BaseRectangle;
        private GameMap _map;
        public List<Cell> Cells { get
            {
                return _cells;
            } }

        public Room(GameMap map)
        {
            _cells = new List<Cell>();
            _map = map;
            BaseRectangle = Rectangle.Empty;
        }
        public Room(GameMap map, Rectangle room) : this(map)
        {
            BaseRectangle = room;
        }

        public void AddCell(Cell cell)
        {
            _cells.Add(cell);
        }

        public bool Contains(ICell cell)
        {
            if (_cells.Contains(cell))
            {
                return true;
            }
            return BaseRectangle.Contains(cell.X, cell.Y);
        }
    }
}
