using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Systems;
using RLNET;
using RogueSharp;
using RogueSharp.DiceNotation;

namespace Projet.Core
{
    public class NeuronMap : GameMap
    {
        public NeuronMap() : base()
        {
        }

        public override Room GetRoom(Cell cell)
        {
            foreach (Room room in Rooms)
            {
                if (room.Contains(cell))
                {
                    return room;
                }
            }
            return Room.Empty;
        }
    }
}
