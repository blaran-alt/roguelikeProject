using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;

namespace Projet.Systems
{
    public class Connection
    {
        public readonly Terminal TerminalA;
        public readonly Terminal TerminalB;
        public bool Output { get
            {
                return Gate();
            } }

        public Connection(Terminal terminal1, Terminal terminal2)
        {
            TerminalA = terminal1;
            TerminalB = terminal2;
        }

        protected virtual bool Gate()
        {
            return TerminalA.isActive && TerminalB.isActive;
        }
    }
}
