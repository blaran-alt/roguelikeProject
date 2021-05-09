using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RogueSharp.DiceNotation;
using RLNET;
using Projet.Systems;
using Projet.Behaviors;

namespace Projet.Monsters
{
    public class Bodybuilder : Monster
    {
        public static Bodybuilder Create(int level)
        {
            int health = Dice.Roll("3D5");
            return new Bodybuilder
            {
                Attack = Dice.Roll("1D2") + level / 3,
                AttackChance = Dice.Roll("25D3"),
                Awareness = 10,
                Color = RLColor.White,
                Defense = Dice.Roll("1D2") + level / 3,
                DefenseChance = Dice.Roll("10D4"),
                Gold = Dice.Roll("5D5"),
                Health = health,
                MaxHealth = 15,
                Name = "Bodybuilder",
                Speed = 15,
                Symbols = new int[] { 258, 258, 258, 259 }
            };
        }

        public override void PerformAction(CommandSystem commandSystem)
        {
            MoveAndDistanceAttack behavior = new MoveAndDistanceAttack();
            behavior.Act(this, commandSystem);
        }
    }
}
