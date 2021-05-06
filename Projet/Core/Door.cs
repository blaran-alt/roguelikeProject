using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using Projet.Interfaces;

namespace Projet.Core
{
    public class Door : Object
    {
        public Door(int x, int y) : base('+', x, y, Colors.Door)
        {
            BackgroundColor = Colors.DoorBackground;
            IsOpen = false;
        }
        public bool IsOpen { get; set; }

        public RLColor BackgroundColor { get; set; }

        public override void Draw(RLConsole console, IMap map, bool animation)
        {
            Symbols = (IsOpen) ? new int[] { '-' } : new int[] { '+' };
            base.Draw(console, map, animation);
        }
    }
}
