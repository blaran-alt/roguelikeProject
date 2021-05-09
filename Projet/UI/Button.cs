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
        public string[] Values { get; set; }
        public bool IsDisabled;
        public event EventHandler<EventArgs> OnClick;
        public EventArgs eventArgs;
        protected Point _topLeftCorner;
        public Point TopLeftCorner { get
            {
                return _topLeftCorner;
            }
        }
        protected Point _bottomRightCorner;
        public Point BottomRightCorner { get
            {
                return _bottomRightCorner;
            }
        }
        protected RLColor _color;
        protected RLColor _hoverColor;
        protected RLColor _disabledColor;
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

        public Button(string value, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor) : this (new string[] { value }, topLeftCorner, bottomRightCorner, color, hoverColor)
        {
        }
        public Button(string value, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor, RLColor disabledColor) : this(new string[] { value }, topLeftCorner, bottomRightCorner, color, hoverColor)
        {
            _disabledColor = disabledColor;
        }
        public Button(string[] values, Point topLeftCorner, Point bottomRightCorner, RLColor color, RLColor hoverColor)
        {
            Values = values;
            _topLeftCorner = topLeftCorner;
            _bottomRightCorner = bottomRightCorner;
            _color = color;
            _hoverColor = hoverColor;
            eventArgs = EventArgs.Empty;
            IsDisabled = false;
        }

        public virtual void Draw(RLConsole console, bool isHovered)
        {
            RLColor color;
            if (isHovered)
            {
                color = _hoverColor;
            }
            else if (IsDisabled)
            {
                color = _disabledColor;
            }
            else
            {
                color = _color;
            }
            console.SetBackColor(TopLeftCorner.X, TopLeftCorner.Y, Width, Height, color);
            string value;
            if(Values.Length == 1)
            {
                value = Values[0];
                console.Print(TopLeftCorner.X + Game.GetCenterOffset(Width, value.Length), TopLeftCorner.Y + Game.GetEvenlySpacedOffset(Height, 1), value, Colors.Text);
            }
            else
            {
                int offset = Game.GetEvenlySpacedOffset(Height, Values.Length);
                
                int i = 1;
                foreach (string text in Values)
                {
                    console.Print(TopLeftCorner.X + Game.GetCenterOffset(Width, text.Length), TopLeftCorner.Y + (offset+1) * i, text, Colors.Text);
                    i++;
                }
            }
            CellSelection.CreateRoomWalls(TopLeftCorner, BottomRightCorner - TopLeftCorner, 179, 196, 218, 191, 192, 217, console);
        }

        public virtual void Click()
        {
            if (!IsDisabled)
            {
                OnClick?.Invoke(this, eventArgs);
            }
        }
    }
}
