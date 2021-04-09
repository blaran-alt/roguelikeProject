using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using Projet.Systems;
using Projet.Behaviors;

namespace Projet.Core
{
    public class Monster : Actor
    {
        public int? TurnsAlerted { get; set; }

        public virtual void PerformAction(CommandSystem commandSystem)
        {
            StandardMoveAndAttack behavior = new StandardMoveAndAttack();
            behavior.Act(this, commandSystem);
        }

        public void DrawStats(RLConsole statConsole, int position)
        {
            int yPosition = 13 + 2 * position;
            statConsole.Print(1, yPosition, Symbol.ToString(), Color);

            int width = Convert.ToInt32(((double)Health / (double)MaxHealth) * 16.0);
            int remainingWidth = 16 - width;

            statConsole.SetBackColor(3, yPosition, width, 1, Colors.Health);
            statConsole.SetBackColor(3 + width, yPosition, remainingWidth, 1, Colors.AlternateDarkest);

            statConsole.Print(2, yPosition, $": {Name}     {Health}/{MaxHealth}", Colors.Text);
        }
    }
}
