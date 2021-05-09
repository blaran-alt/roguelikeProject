using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RogueSharp;
using RLNET;

namespace Projet.Systems
{
    public class InvertedMap : GameMap
    {
        private List<DropPad> DropPads;

        public InvertedMap() : base() 
        {
            DropPads = new List<DropPad>();
        }

        protected override void DrawPuzzlePieces(RLConsole console)
        {
            foreach (DropPad pad in DropPads)
            {
                pad.Draw(console, this, false);
            }
            foreach (Box box in Boxs)
            {
                box.Draw(console, this, false);
            }
        }

        protected override bool NextLevelCondition()
        {
            foreach (DropPad pad in DropPads)
            {
                Box box = GetBoxAt(pad.Coord);
                if(box == null || box.Code != pad.Code)
                {
                    return false;
                }
            }
            if (!StairsDown.IsOpen)
            {
                Game.ReloadBitmap();
                ObstacleFree = true;
                StairsDown.IsOpen = true;
            }
            return true;
        }

        public RLConsole InvertMap(RLConsole mapConsole)
        {
            RLConsole upsideDownConsole = new RLConsole(Width,Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    RLConsole.Blit(mapConsole, x, y, 1, 1, upsideDownConsole, Width - 1 - x, Height - 1 - y);
                }
            }
            return upsideDownConsole;
        }

        public DropPad GetDropPadAt(int x,int y)
        {
            return DropPads.FirstOrDefault(b => b.X == x && b.Y == y);
        }

        public void AddDropPad(DropPad pad)
        {
            DropPads.Add(pad);
        }
    }
}
