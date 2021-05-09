using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RLNET;

namespace Projet.Items
{
    public class Key : Item
    {
        private readonly RLColor[] keyColors = new RLColor[] {Colors.Gold, RLColor.Gray, RLColor.White };

        public Key(int x, int y, int effectCode)
        {
            Name = "Clef";
            Quantity = 1;
            X = x;
            Y = y;
            EffectCode = effectCode;
            Color = keyColors[effectCode];
            Symbols = new int[]{'µ'};
        }
        public Key(int effectCode) : this(0, 0, effectCode) { }

        public override void Draw(RLConsole console, int x, int y)
        {
            console.Set(x, y, Color, null, Symbols[0]);
            console.Print(x + 1, y, $" - {EffectCode}", Colors.Text);
        }
    }
}
