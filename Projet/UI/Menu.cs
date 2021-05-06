using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;

namespace Projet.UI
{
    public class Menu
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public static Point _mousePos;
        protected readonly string _name;
        protected List<Button> _buttons;

        public Menu(List<Button> buttons, string menuName, int width, int height)
        {
            _buttons = buttons;
            _name = menuName;
            Width = width;
            Height = height;
        }

        public virtual void Draw(RLConsole console, Point mousePos)
        {
            _mousePos = mousePos;
            console.Print(Game.GetCenterOffset(console.Width, _name.Length), 5, _name, Colors.TextHeading);
            foreach (Button button in _buttons)
            {
                button.Draw(console, IsOverButton(button));
            }
        }

        private bool IsOverButton(Button button)
        {
            return (_mousePos.X > button.TopLeftCorner.X && _mousePos.X < button.BottomRightCorner.X && _mousePos.Y > button.TopLeftCorner.Y && _mousePos.Y < button.BottomRightCorner.Y);
        }
        public static bool StaticIsOverButton(Button button)
        {
            return (_mousePos.X > button.TopLeftCorner.X && _mousePos.X < button.BottomRightCorner.X && _mousePos.Y > button.TopLeftCorner.Y && _mousePos.Y < button.BottomRightCorner.Y);
        }

        public bool Click()
        {
            foreach (Button button in _buttons)
            {
                if (IsOverButton(button))
                {
                    button.Click();
                    return true;
                }
            }
            return false;
        }
    }
}
