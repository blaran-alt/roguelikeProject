﻿using System;
using RLNET;
using RogueSharp;
using Projet.Interfaces;

namespace Projet.Core
{
    public abstract class Actor : IActor, IDrawable, IScheduleable
    {
        // IScheduleable
        public int Time { get { return Speed; } }

        // IActor
        private int _attack;
        private int _attackChance;
        private int _awareness;
        private int _defense;
        private int _defenseChance;
        private int _gold;
        private int _health;
        private int _maxHealth;
        private string _name;
        private int _speed;

        public int AnimationIndex { get; set; }

        public int Attack
        {
            get
            {
                return _attack;
            }
            set
            {
                _attack = value;
            }
        }

        public int AttackChance
        {
            get
            {
                return _attackChance;
            }
            set
            {
                _attackChance = value;
            }
        }

        public int Awareness
        {
            get
            {
                return _awareness;
            }
            set
            {
                _awareness = value;
            }
        }

        public int Defense
        {
            get
            {
                return _defense;
            }
            set
            {
                _defense = value;
            }
        }

        public int DefenseChance
        {
            get
            {
                return _defenseChance;
            }
            set
            {
                _defenseChance = value;
            }
        }

        public int Gold
        {
            get
            {
                return _gold;
            }
            set
            {
                _gold = value;
            }
        }

        public int Health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = value;
            }
        }

        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                _maxHealth = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
            }
        }

        // IDrawable
        public RLColor Color { get; set; }
        public int[] Symbols { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Point Coord
        {
            get
            {
                return new Point(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public virtual void Draw(RLConsole console, IMap map, bool animation)
        {
            // Don't draw actors in cells that haven't been explored
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            // Only draw the actor with the color and symbol when they are in field-of-view
            if (map.IsInFov(X, Y))
            {
                console.Set(X, Y, Color, Colors.FloorBackgroundFov, GetNextAnimation(animation));
            }
            else if(Game.Map.ObstacleFree)
            {
                // When not in field-of-view just draw a normal floor
                console.Set(X, Y, Colors.Floor, Colors.FloorBackground, '.');
            }
        }

        protected virtual int GetNextAnimation(bool nextAnimation)
        {
            int animation = Symbols[AnimationIndex];
            if (nextAnimation && ++AnimationIndex >= Symbols.Length)
            {
                AnimationIndex = 0;
            }
            return animation;
        }
    }
}
