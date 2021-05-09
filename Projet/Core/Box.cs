﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using Projet.Interfaces;
using RLNET;
using RogueSharp;

namespace Projet.Core
{
    public class Box : Object
    {
        public readonly int Code;

        public Box(int x, int y, int code) : base((char)219, x, y, Colors.TerminalColors[code])
        {
            Code = code;
        }
    }
}
