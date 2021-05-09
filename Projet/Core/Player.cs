using System;
using RLNET;
using RogueSharp;
using Projet.Interfaces;
using System.Collections.Generic;

namespace Projet.Core
{
    public class Player : Actor
    {
        public Player()
        {
            Attack = 2;
            AttackChance = 50;
            Awareness = 15;
            Color = Colors.Player;
            Defense = 2;
            DefenseChance = 40;
            Gold = 0;
            Health = 20;
            MaxHealth = 100;
            Name = "Reanimateur";
            Speed = 10;
            Symbols = new int[] { 64, 251, 1, 2,  1, 251 };
            AnimationIndex = 0;
            speedTimers = new List<SpeedAffects>();
        }

        public void DrawStats(RLConsole statConsole)
        {
            statConsole.Print(1, 1, $"Nom: {Name}", Colors.Text);
            statConsole.Print(1, 3, $"Sante: {Health}/{MaxHealth}", Colors.Text);
            statConsole.Print(1, 5, $"Attaque: {Attack} ({AttackChance}%)", Colors.Text);
            statConsole.Print(1, 7, $"Defence: {Defense} ({DefenseChance}%)", Colors.Text);
            statConsole.Print(1, 9, $"Point: {Gold}", Colors.Gold);
        }

        private List<SpeedAffects> speedTimers;

        public void AffectSpeed(int amount, int time)
        {
            speedTimers.Add(new SpeedAffects(time, amount));
            Speed = Game.Clamp(Speed - amount, 5, 15);
        }

        public void Reset()
        {
            Health = MaxHealth;
            Gold = 0;
        }

        public override void Draw(RLConsole console, IMap map, bool nextAnimation)
        {
            console.Set(X, Y, Color, Colors.FloorBackgroundFov, Symbols[AnimationIndex]);
            if (nextAnimation && ++AnimationIndex >= Symbols.Length)
            {
                AnimationIndex = 0;
            }
            foreach(SpeedAffects affect in speedTimers)
            {
                if (affect.IsDone())
                {
                    Speed = Game.Clamp(Speed + affect.Amount, 5, 15);
                    speedTimers.Remove(affect);
                    Console.WriteLine(speedTimers.Count);
                }
            }
        }

        private struct SpeedAffects
        {
            public SpeedAffects(int timer, int amount)
            {
                _timer = timer;
                Amount = amount;
            }
            private int _timer;
            public readonly int Amount;
            public bool IsDone()
            {
                if (--_timer == 0)
                {
                    return true;
                }
                return false;
            }

        }
    }
}
