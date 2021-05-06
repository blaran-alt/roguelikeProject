using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using RLNET;
using Projet.Systems;

namespace Projet.UI
{
    class TextArea : Button
    {
        public string DefaultValue;
        private Button submitButton;
        public bool EnterPressed;
        public string Value { get; set; }

        public TextArea(string defaultValue, Point topLeftCorner, Point bottomRightCorner, RLColor submitColor, RLColor submitHoverColor) : base(defaultValue, topLeftCorner, bottomRightCorner, RLColor.White, RLColor.Gray)
        {
            EnterPressed = false;
            DefaultValue = defaultValue;
            Value = defaultValue;
            if(Height < 3)
            {
                _bottomRightCorner.Y = topLeftCorner.Y + 3;
            }
            submitButton = new Button("Valider", TopLeftCorner + new Point((int)Math.Floor(Width * .7), 0), BottomRightCorner, submitColor, submitHoverColor);
            submitButton.OnClick += OnSubmit;
            OnClick += Game.TakeInput;
        }

        public override void Draw(RLConsole console, bool isHovered)
        {
            submitButton.Draw(console, isHovered);
            RLColor color;
            if (isHovered)
            {
                color = _hoverColor;
            }
            else
            {
                color = _color;
            }
            console.SetBackColor(TopLeftCorner.X, TopLeftCorner.Y, (int)Math.Floor(Width * .7), Height, color);
            string value;
            if (Value == null)
            {
                value = _symbol.ToString();
            }
            else
            {
                value = Value;
            }
            console.Print(TopLeftCorner.X + Game.GetCenterOffset((int)Math.Floor(Width * .7), value.Length), TopLeftCorner.Y + Game.GetEvenlySpacedOffset(Height, 1), value, Colors.Text);
        }

        public override void Click()
        {
            if (Menu.StaticIsOverButton(submitButton) || EnterPressed)
            {
                if (Value == DefaultValue)
                {
                    Game.MessageLog.Add("Vous devez rentrez un champ");
                }
                else
                {
                    Game.StartWithSeed(int.Parse(Value));
                }
                EnterPressed = false;
            }
            else
            {
                base.Click();
            }
        }

        private void OnSubmit(Object sender, EventArgs args)
        {
            Click();
        }
    }
}
