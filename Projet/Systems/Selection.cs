using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using Projet.Core;

namespace Projet.Systems
{
    public class Selection
    {
        public int X { get; set; }
        public int Y { get; set; }
        private int _defaultX;
        private int _defaultY;
        private int _containerCapacity;

        public Selection(int defaultX,int defaultY, int containerCapacity)
        {
            _defaultX = defaultX - 1;
            _defaultY = defaultY - 1;
            _containerCapacity = containerCapacity;;
        }

        public void Draw(RLConsole console)
        {
            int width = console.Width - 2;
            int height = console.Height - 3;
            int _columnWidth = (width * height) / (2 * _containerCapacity) - 1;
            int _lineWidth = 2;
            console.Set(X, Y, _columnWidth, _lineWidth, null ,Colors.ComplimentDarkest,null);
        }

        public void Reset()
        {
            X = _defaultX;
            Y = _defaultY;
        }
    }
}
