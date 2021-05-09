using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using Projet.Systems;
using RogueSharp.DiceNotation;

namespace Projet.Core
{
    public class CortexMap : GameMap
    {
        public CortexMap() : base()
        {
            ObstacleFree = false;

        }

        protected override bool NextLevelCondition()
        {
            foreach (Connection connection in Connections)
            {
                if (!connection.Output)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Draw(RLConsole mapConsole, RLConsole statConsole, bool NextAnimation)
        {
            ObstacleFree = StairsDown.IsOpen;
            base.Draw(mapConsole, statConsole, NextAnimation);
        }

        protected override void DrawPuzzlePieces(RLConsole console)
        {
            foreach (Connection connection in Connections)
            {
                connection.TerminalA.Draw(console, this, false);
                connection.TerminalB.Draw(console, this, false);
            }
        }
    }
}
