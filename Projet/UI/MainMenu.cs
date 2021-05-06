using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;

namespace Projet.UI
{
    public class MainMenu : Menu
    {
        public MainMenu(int width, int height) : base(new List<Button>(), "Menu Principal", width, height)
        {
            Button playButton = new Button("Jouer", Game.GetProportionnalSize(20,25), Game.GetProportionnalSize(80, 50), Colors.Compliment, Colors.ComplimentDarker);
            playButton.OnClick += Game.Start;
            Button playWithSeedButton = new Button("Jouer une seed", Game.GetProportionnalSize(20, 55), Game.GetProportionnalSize(80, 70), Colors.Compliment, Colors.ComplimentDarker);
            playWithSeedButton.OnClick += Game.OpenSeedMenu;
            Button quitButton = new Button("Quitter", Game.GetProportionnalSize(20, 75), Game.GetProportionnalSize(80, 90), Colors.Compliment, Colors.ComplimentDarker);
            quitButton.OnClick += Game.Quit;
            _buttons.Add(playButton);
            _buttons.Add(playWithSeedButton);
            _buttons.Add(quitButton);
        }
    }
}
