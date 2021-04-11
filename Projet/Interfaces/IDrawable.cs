using System;
using RLNET;
using RogueSharp;

namespace Projet.Interfaces
{
    public interface IDrawable
    {
        RLColor Color { get; set; }
        char Symbol { get; set; }
        int X { get; set; }
        int Y { get; set; }
        Point Coord { get; set; }

        void Draw(RLConsole console, IMap map);
    }
}
