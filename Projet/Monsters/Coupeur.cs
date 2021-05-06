using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RogueSharp.DiceNotation;
using RLNET;

namespace Projet.Monsters
{
    public class Coupeur : Monster
    {
        public static Coupeur Create(int level)
        {
            int health = Dice.Roll("2D5");
            return new Coupeur
            {
                Attack = Dice.Roll("1D3") + level / 3,
                AttackChance = Dice.Roll("25D3"),
                Awareness = 10,
                Color = RLColor.White,
                Defense = Dice.Roll("1D3") + level / 3,
                DefenseChance = Dice.Roll("10D4"),
                Gold = Dice.Roll("5D5"),
                Health = health,
                MaxHealth = 10,
                Name = "Coupeur",
                Speed = 14,
                Symbols = new int[] { 256, 256, 256, 257}
            };
        }
    }
}
