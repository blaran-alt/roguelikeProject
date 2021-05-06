using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Interfaces;
using RogueSharp;
using RLNET;

namespace Projet.Core
{
    public abstract class Object : IDrawable
    {
        public RLColor Color { get; set; }
        public int[] Symbols { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int AnimationIndex { get; set; }
        public Point Coord
        {
            get
            {
                return new Point(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Object(char symbol, int x, int y, RLColor color)
        {
            Symbols = new int[] { symbol };
            X = x;
            Y = y;
            Color = color;
        }
        public Object(char symbol, Point coord, RLColor color)
        {
            Symbols = new int[] { symbol };
            Coord = coord;
            Color = color;
        }

        public virtual void Draw(RLConsole console, IMap map, bool animation)
        {
            // Don't draw actors in cells that haven't been explored
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            // Only draw the actor with the color and symbol when they are in field-of-view
            if (map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Colors.FloorBackgroundFov, Symbols[0]);
            }
            else if(Game.Map.LightsOn)
            {
                // When not in field-of-view just draw a normal floor
                console.Set(X, Y, Colors.Floor, Colors.FloorBackground, '.');
            }
        }
    }
}
