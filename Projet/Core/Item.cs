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
    public abstract class Item : IItem, IDrawable
    {
        // IItem
        public string Name { get; set; }
        public int EffectCode { get; set; }
        public bool Dropped { get; set; }
        public int Quantity { get; set; }

        // IDrawable
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
        public void Draw(RLConsole console, IMap map, bool animation)
        {
            if (map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Colors.FloorBackgroundFov, Symbols[AnimationIndex]);
            }
        }

        public void DrawInContainer(RLConsole console, int x, int y)
        {
            console.Set(x, y, Color, null, Symbols[0]);
            console.Print(x + 1, y,$" - {Quantity}", Colors.Text);
        }

        public virtual bool Use()
        {
            return false;
        }

        public virtual void AlternateDrawInContainer(RLConsole console,int x, int y)
        {
            DrawInContainer(console, x, y);
        }

        public bool IsEqual(Item item)
        {
            return item.Name == Name && item.EffectCode == EffectCode;
        }

        public override string ToString()
        {
            return $"{Name} : effect = {EffectCode}, quantity = {Quantity} and (x,y) = ({X},{Y})";
        }
    }
}
