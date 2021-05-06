using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;

namespace Projet.UI
{
    class TextArea : Button
    {
        public string DefaultValue;
        private Button submitButton;

        public TextArea(string defaultValue, Point topLeftCorner, Point bottomRightCorner, RLColor submitColor, RLColor submitHoverColor) : base(defaultValue, topLeftCorner, bottomRightCorner, RLColor.White, RLColor.Gray)
        {
            DefaultValue = defaultValue;
            submitButton = new Button("Valider", TopLeftCorner + new Point(Width + 2, 0), BottomRightCorner + new Point(8, 0), submitColor, submitHoverColor);
            submitButton.OnClick += OnSubmit;
        }

        public override void Click()
        {
            if(Value == DefaultValue)
            {
                Game.MessageLog.Add("Vous devez rentrez un texte");
            }
            else
            {
                eventArgs = new MenuEventArgs(Value);
                base.Click();
            }
        }

        private void OnSubmit(Object sender, EventArgs args)
        {
            Click();
        }
    }
}
