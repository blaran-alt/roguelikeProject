using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;

namespace Projet.Items
{
    public class Potion : Item
    {
        public Potion(bool dropped, int x, int y, int effectCode)
        {
            Name = "Potion";
            Quantity = 1;
            Dropped = dropped;
            X = x;
            Y = y;
            EffectCode = effectCode;
            Color = Colors.ComplimentLightest;
            Symbol = '6';
        }
        public Potion(int effectCode, int x, int y ) : this (true, x, y, effectCode) { }
        public Potion(int effectCode) : this(false, 0, 0, effectCode) { }
    }
}
