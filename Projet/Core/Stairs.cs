using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using Projet.Interfaces;
using RogueSharp;

namespace Projet.Core
{
    public class Stairs : Object
    {
        public bool IsUp
        {
            get;
        }
        public bool IsOpen
        {
            get;
            set;
        }

        public Stairs(int x, int y, bool isUp) : base(isUp ? '<' : '_', x, y, RLColor.Black)
        {
            IsUp = isUp;
            if (isUp)
            {
                IsOpen = true;
            }
            else
            {
                IsOpen = false;
            }
        }

        public override void Draw(RLConsole console, IMap map, bool animation)
        {
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            if (IsOpen)
            {
                Symbols = new int[] { '>' };
            }

            if (map.IsInFov(X, Y))
            {
                Color = Colors.Player;
                console.Set(X, Y, Color, null, Symbols[0]);
            }
            else if(Game.Map.LightsOn)
            {
                Color = Colors.Floor;
                console.Set(X, Y, Color, null, Symbols[0]);
            }
        }
    }
}
