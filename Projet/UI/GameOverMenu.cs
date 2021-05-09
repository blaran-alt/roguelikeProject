using RLNET;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.UI
{
    public class GameOverMenu : Menu
    {

        public GameOverMenu(int width, int height):base(new List<Button>(), "GAME OVER", width, height)
        {
            Point size = new Point(width, height);
            TextArea saveName = new TextArea("Nom de la sauvegarde",Game.GetProportionnalSize(10, 60),Game.GetProportionnalSize(90, 70), Colors.ComplimentLighter, Colors.Compliment);
            _buttons.Add(saveName);
            saveName.OnClick += Game.TakeInput;
            saveName.OnSubmit += Game.SaveData;
            Button replayButton = new Button("Rejouer",  Game.GetProportionnalSize(30, 80), Game.GetProportionnalSize(70, 90), Colors.ComplimentLightest, Colors.ComplimentLighter);
            _buttons.Add(replayButton);
            replayButton.OnClick += Game.Start;
        }

        public override void Draw(RLConsole console, Point mousePos)
        {
            base.Draw(console, mousePos);
            int printY = Game.GetProportionnalVerticalSize(20);
            console.Print(Game.GetCenterOffset(Width, 36),printY, "Entrez un nom et cliquer sur Valider", Colors.Text);
            printY = Math.Max(printY + 1, Game.GetProportionnalVerticalSize(25));
            console.Print(Game.GetCenterOffset(Width, 37), printY, "pour enregistrer la seed de la partie", Colors.Text);
        }
    }
}
