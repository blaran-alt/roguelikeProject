﻿using System;
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

        public Key(bool dropped, int x, int y, int effectCode)
        {
            Name = "Clef";
            Quantity = 1;
            Dropped = dropped;
            X = x;
            Y = y;
            EffectCode = effectCode;
            Color = keyColors[effectCode];
            Symbols = new int[]{'µ'};
        }
        public Key(int effectCode) : this(false, 0, 0, effectCode) { }
        public Key(int effectCode, int x, int y) : this(true, x, y, effectCode) { }

        public override void AlternateDrawInContainer(RLConsole console, int x, int y)
        {
            console.Set(x, y, Color, null, Symbols[0]);
            console.Print(x + 1, y, $" - {EffectCode}", Colors.Text);
        }
    }
}
