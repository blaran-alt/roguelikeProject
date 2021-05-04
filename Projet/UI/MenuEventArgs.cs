using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.UI
{
    public class MenuEventArgs : EventArgs
    {
        public bool Value { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }

        public MenuEventArgs(bool value, int index, string name)
        {
            Value = value;
            Index = index;
            Name = name;
        }
        public MenuEventArgs() : this (false, 0, "") { }
        public MenuEventArgs(int index) : this(false, index, "") { }
        public MenuEventArgs(string name) : this(false, 0, name) { }
        public MenuEventArgs(bool value) : this(value, 0, "") { }
    }
}
