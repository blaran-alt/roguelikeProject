using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RogueSharp;
using RLNET;
using Projet.Systems;

namespace Projet.Items
{
    public class Potion : Item
    {
        private readonly string _effect;
        private readonly string[] _effects = new string[] { "Sante", "Vitesse", "Degats" };

        private readonly RLColor[] colors = new RLColor[] {Colors.ComplimentLightest, Colors.AlternateDarkest, Colors.ComplimentDarkest};
        public Potion(int x, int y, int effectCode)
        {
            Name = "Potion";
            Quantity = 1;
            X = x;
            Y = y;
            EffectCode = effectCode;
            Color = colors[effectCode];
            Symbols = new int[] { '^' };
            _effect = _effects[effectCode];
        }

        public override bool Use()
        {
            Player player = Game.Player;
            GameMap map = Game.Map;
            if (EffectCode == 0)
            {
                player.Health = Math.Min(player.Health + 10, player.MaxHealth);
                Game.MessageLog.Add($"{player.Name} a utilise une potion de sante et gagne 10 hp");
                return true;
            }
            else if(EffectCode == 1)
            {
                player.AffectSpeed(5, 7);
                Game.MessageLog.Add($"{player.Name} a utilise une potion de vitesse et double sa vitesse pour 5 tours");
                return true;
            }
            else if(EffectCode == 2)
            {
                Game.MessageLog.Add($"{player.Name} a utilise une potion de degats");
                CellSelection.StartShochWaveEffect(player.Coord, 5);
                IEnumerable<ICell> surroundingCells = map.GetCellsInCircle(Game.Player.X, Game.Player.Y, 5);
                if (surroundingCells != null)
                {
                    foreach (ICell cell in surroundingCells)
                    {
                        Monster monster = map.GetMonsterAt(cell.X, cell.Y);
                        if (monster != null && Game.Map.IsInFov(cell.X, cell.Y))
                        {
                            Game.CommandSystem.ResolveDamage(monster, 3);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public override void Draw(RLConsole console, int x, int y)
        {
            console.Set(x, y, Color, null, Symbols[0]);
            console.Print(x + 3, y, _effect, Colors.Text, Colors.ComplimentDarkest);
            console.Print(x + 2, y + 1, Quantity.ToString(), Colors.Text, Colors.ComplimentDarkest);
        }
    }
}
