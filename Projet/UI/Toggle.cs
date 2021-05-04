using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;

namespace Projet.UI
{
    public class Toggle : Button
    {
        private bool _value;

        public Toggle(string name, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor) : base(name, topLeftCorner, bottomRightCorner, color, hoverColor)
        {
            _value = false;
        }
        public Toggle(char symbol, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor) : base (symbol, topLeftCorner, bottomRightCorner, color, hoverColor)
        {
            _value = false;
        }
        public Toggle(string name, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor, bool value) : base(name, topLeftCorner, bottomRightCorner, color, hoverColor)
        {
            _value = value;
        }
        public Toggle(char symbol, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor, bool value) : base(symbol, topLeftCorner, bottomRightCorner, color, hoverColor)
        {
            _value = value;
        }

        public override void Click()
        {
            _value = !_value;
            eventArgs = new MenuEventArgs(_value);
            base.Click();
        }
    }
}
