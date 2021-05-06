using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using Projet.Systems;

namespace Projet.UI
{
    public class ChoseLevel : Menu
    {
        private Save _save;

        public ChoseLevel(int width, int height, Save save) : base(new List<Button>(), "Choisis un niveau", width, height)
        {
            _save = save;

            int buttonsHeight = Math.Min(((height - Game.GetProportionnalVerticalSize(50)) - (save.Seeds.Count - 1) * 2) * 4 / save.Seeds.Count, 7);
            Point topLeft = Game.GetProportionnalSize(1, 30);

            int i = 0;
            foreach (int seed in save.Seeds)
            {
                Button button = new Button(new string[] { "Level " + (i + 1).ToString(), save.Seeds[i].ToString()}, topLeft, topLeft + new Point(width / 4 - 2, buttonsHeight), Colors.Alternate, Colors.AlternateDarker)
                {
                    eventArgs = new MenuEventArgs(save.Seeds[i])
                };
                button.OnClick += OnSeedChosen;
                _buttons.Add(button);
                topLeft.X += width / 4;
                if (topLeft.X > width)
                {
                    topLeft.X = 1;
                    topLeft.Y += buttonsHeight + 1;
                }
                i++;
            }
        }

        private void OnSeedChosen(object sender, EventArgs args)
        {
            MenuEventArgs menuArgs = args as MenuEventArgs;
            Game.StartWithSeed(menuArgs.IntValue);
        }
    }
}
