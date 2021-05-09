using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using Projet.Core;

namespace Projet.Systems
{
    public class UISelection
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int SelectedItemIndex { get; set; }
        private readonly int _defaultX;
        private readonly int _defaultY;

        public UISelection(int defaultX,int defaultY)
        {
            _defaultX = defaultX - 1;
            _defaultY = defaultY - 1;
            Reset();
        }

        public void Draw(RLConsole console)
        {
            int _columnWidth = console.Width - 2;
            int _lineWidth = 3;
            console.Set(X, Y, _columnWidth, _lineWidth, null ,Colors.ComplimentDarkest,null);
        }

        public bool MoveSelection(Direction direction, int itemsNb)
        {
            int targetY;

            switch (direction)
            {
                case Direction.Up:
                    {
                        targetY =  Y - 2;
                        break;
                    }
                case Direction.Down:
                    {
                        targetY = Y + 2;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            if(targetY >= _defaultY && targetY < _defaultY + itemsNb * 2)
            {
                Y = targetY;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            X = _defaultX;
            Y = _defaultY;
            SelectedItemIndex = 0;
        }
    }
}
