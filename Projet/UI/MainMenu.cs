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
        public MainMenu(int width, int height) : base(new List<Button>(), "Main Menu", width, height)
        {
            Button playButton = new Button("Play", Game.GetPropostionnalSize(20,15), Game.GetPropostionnalSize(80, 30), Colors.Compliment, Colors.ComplimentDarker);
            playButton.OnClick += Game.Start;
            Button playWithSeedButton = new Button("Play with seed", Game.GetPropostionnalSize(20, 32), Game.GetPropostionnalSize(80, 42), Colors.Compliment, Colors.ComplimentDarker);
            Button quitButton = new Button("Quit", Game.GetPropostionnalSize(20, 44), Game.GetPropostionnalSize(80, 49), Colors.Compliment, Colors.ComplimentDarker);
            quitButton.OnClick += Game.Quit;
            _buttons.Add(playButton);
            _buttons.Add(playWithSeedButton);
            _buttons.Add(quitButton);
        }
    }
}
