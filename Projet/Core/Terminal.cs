using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using Projet.Interfaces;
using RogueSharp.DiceNotation;

namespace Projet.Core
{
    public class Terminal : Object
    {
        public bool isActive;

        public Terminal(int x, int y, RLColor color) : base((char)10, x, y, color)
        {
            isActive = Dice.Roll("2D3") < 3;
        }

        public override void Draw(RLConsole console, IMap map, bool animation)
        {
            Symbols = isActive ? new int[] { 10 } : new int[] { 8 };
            base.Draw(console, map, animation);
        }
    }
}
