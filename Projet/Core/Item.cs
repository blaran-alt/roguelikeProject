using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Interfaces;
using RLNET;
using RogueSharp;

namespace Projet.Core
{
    public class Item : IItem, IDrawable
    {
        // IItem
        public string Name { get; set; }
        public int EffectCode { get; set; }
        public bool Dropped { get; set; }
        public int Quantity { get; set; }

        // IDrawable
        public RLColor Color { get; set; }
        public char Symbol { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public void Draw(RLConsole console, IMap map)
        {
            if (map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Colors.FloorBackgroundFov, Symbol);
            }
        }

        public void DrawInContainer(RLConsole console, int x, int y)
        {
            console.Set(x, y, Color, null, Symbol);
            console.Print(x + 1, y,$" - {Quantity.ToString()}", Colors.Text);
        }
    }
}
