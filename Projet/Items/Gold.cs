using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RogueSharp.DiceNotation;

namespace Projet.Items
{
    public class Gold : Item
    {
        public Gold(int quantity, bool dropped, int x, int y)
        {
            Name = "Gold";
            EffectCode = -1;
            Dropped = dropped;
            Quantity = quantity;
            Color = Colors.Gold;
            Symbol = 'G';
            X = x;
            Y = y;
        }
        public Gold(int quantity, int x, int y) : this(quantity, true, x, y) { }
        public Gold(int quantity) : this(quantity, false, 0, 0) { }
    }
}
