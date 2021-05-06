using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Systems;
using RogueSharp;
using RLNET;

namespace Projet.UI
{
    public class ChoseSeedMenu : Menu
    {
        private List<Save> saves;

        public ChoseSeedMenu(int width, int height) : base(new List<Button>(), "Chose Seed", width, height)
        {
            saves = new List<Save>();
            bool last;
            int i = 0;
            do
            {
                Save save = new Save(i, out last);
                if (!last)
                {
                    saves.Add(save);
                    i++;
                }
            } while (!last);

            int buttonsHeight = Math.Min(((height - Game.GetProportionnalVerticalSize(50)) - (saves.Count - 1) * 2) * 4 / saves.Count, 7);
            Point topLeft = Game.GetProportionnalSize(1, 30);

            foreach (Save save in saves)
            {
                Button button = new Button(save.Name, topLeft, topLeft + new Point(width / 4 - 2, buttonsHeight), Colors.Alternate, Colors.AlternateDarker)
                {
                    eventArgs = new MenuEventArgs(save.Name)
                };
                button.OnClick += OnSaveChosen;
                _buttons.Add(button);
                topLeft.X += width / 4;
                if (topLeft.X > width)
                {
                    topLeft.X = 1;
                    topLeft.Y += buttonsHeight + 1;
                }
            }
            TextArea input = new TextArea("0000", Game.GetProportionnalSize(20,90), Game.GetProportionnalSize(80, 95), Colors.AlternateLightest, Colors.AlternateLighter);
            
            _buttons.Add(input);
        }

        public override void Draw(RLConsole console, Point mousePos)
        {
            console.Print(Game.GetCenterOffset(console.Width, 21), Height - Game.GetProportionnalVerticalSize(15), "Entrer une seed ici :", Colors.TextHeading);
            base.Draw(console, mousePos);
        }

        private void OnSaveChosen(Object sender, EventArgs args)
        {
            MenuEventArgs menuArgs = args as MenuEventArgs;
            Save save = saves.FirstOrDefault(s => s.Name == menuArgs.StringValue);
            Game.OpenSaveMenu(save);
        }

        private void OnSeedChosen(Object sender, EventArgs args)
        {
            MenuEventArgs menuArgs = args as MenuEventArgs;
        }
    }
}
