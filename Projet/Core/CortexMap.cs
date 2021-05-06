using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using Projet.Systems;

namespace Projet.Core
{
    public class CortexMap : GameMap
    {
        public CortexMap() : base()
        {
            LightsOn = false;
        }

        public override bool NextLevelCondition()
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
            LightsOn = StairsDown.IsOpen;
            base.Draw(mapConsole, statConsole, NextAnimation);
        }
    }
}
