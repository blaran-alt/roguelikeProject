using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using Projet.Systems;

namespace Projet.UI
{
    public class Button
    {
        public string Value { get; set; }
        private char _symbol;
        public event EventHandler<EventArgs> OnClick;
        protected EventArgs eventArgs;
        public Point TopLeftCorner { get; set; }
        public Point BottomRightCorner { get; set; }
        private RLColor _color;
        private RLColor _hoverColor;
        protected int Width
        {
            get
            {
                return BottomRightCorner.X - TopLeftCorner.X;
            }
        }
        protected int Height
        {
            get
            {
                return BottomRightCorner.Y - TopLeftCorner.Y;
            }
        }

        public Button(string value, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor)
        {
            Value = value;
            _symbol = ' ';
            TopLeftCorner = topLeftCorner;
            BottomRightCorner = bottomRightCorner;
            _color = color;
            _hoverColor = hoverColor;
            eventArgs = EventArgs.Empty;
        }
        public Button(char symbol, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor)
        {
            Value = "";
            _symbol = symbol;
            TopLeftCorner = topLeftCorner;
            BottomRightCorner = bottomRightCorner;
            _color = color;
            _hoverColor = hoverColor;
            eventArgs = EventArgs.Empty;
        }

        public  void Draw(RLConsole console, bool isHovered)
        {
            RLColor color;
            if (isHovered)
            {
                color = _hoverColor;
            }
            else
            {
                color = _color;
            }
            console.SetBackColor(TopLeftCorner.X, TopLeftCorner.Y, Width, Height, color);
            string value;
            int displayX;
            if(Value == "")
            {
                value = _symbol.ToString();
                displayX = TopLeftCorner.X + Width / 2;
            }
            else
            {
                value = Value;
                displayX = (TopLeftCorner.X + BottomRightCorner.X - value.Length) / 2;
            }
            console.Print(displayX, (BottomRightCorner.Y +   TopLeftCorner.Y)/2 , value, Colors.Text);
            CellSelection.CreateRoomWalls(TopLeftCorner, BottomRightCorner - TopLeftCorner, 179, 196, 218, 191, 192, 217, console);
        }

        public virtual void Click()
        {
            OnClick?.Invoke(this, eventArgs);
        }
    }
}
