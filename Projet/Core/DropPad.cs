using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.Core
{
    public class DropPad : Object
    {
        public readonly int Code;

        public DropPad(int x, int y, int code) : base((char)176, x, y, Colors.TerminalColors[code])
        {
            Code = code;
        }
    }
}
