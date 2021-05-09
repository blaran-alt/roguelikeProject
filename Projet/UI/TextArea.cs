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
        public event EventHandler<MenuEventArgs> OnSubmit;
        public MenuEventArgs menuArgs;

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
            submitButton.OnClick += Submitted;
            OnClick += Game.TakeInput;
        }
        public TextArea(string defaultValue, Point topLeftCorner, Point bottomRightCorner, RLColor submitColor, RLColor submitHoverColor, RLColor submitDisabledColor) : base(defaultValue, topLeftCorner, bottomRightCorner, RLColor.White, RLColor.Gray)
        {
            EnterPressed = false;
            DefaultValue = defaultValue;
            Value = defaultValue;
            if (Height < 3)
            {
                _bottomRightCorner.Y = topLeftCorner.Y + 3;
            }
            submitButton = new Button("Valider", TopLeftCorner + new Point((int)Math.Floor(Width * .7), 0), BottomRightCorner, submitColor, submitHoverColor, submitDisabledColor);
            submitButton.OnClick += Submitted;
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
            else if (IsDisabled)
            {
                color = RLColor.Gray;
            }
            else
            {
                color = _color;
            }
            console.SetBackColor(TopLeftCorner.X, TopLeftCorner.Y, (int)Math.Floor(Width * .7), Height, color);
            console.Print(TopLeftCorner.X + Game.GetCenterOffset((int)Math.Floor(Width * .7), Value.Length), TopLeftCorner.Y + Game.GetEvenlySpacedOffset(Height, 1), Value, Colors.Text);
        }

        public override void Click()
        {
            if (Menu.StaticIsOverButton(submitButton) || EnterPressed)
            {
                if (Value != DefaultValue && !IsDisabled)
                {
                    menuArgs = new MenuEventArgs(Value);
                    OnSubmit?.Invoke(this, menuArgs);
                }
                EnterPressed = false;
            }
            else
            {
                base.Click();
            }
        }

        private void Submitted(Object sender, EventArgs args)
        {
            if (!IsDisabled)
            {
                Click();
            }
        }
    }
}
