using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.UI
{
    public class MenuEventArgs : EventArgs
    {
        public bool BoolValue { get; set; }
        public int IntValue { get; set; }
        public string StringValue { get; set; }

        public MenuEventArgs(bool value, int index, string name)
        {
            BoolValue = value;
            IntValue = index;
            StringValue = name;
        }
        public MenuEventArgs() : this (false, 0, "") { }
        public MenuEventArgs(int index) : this(false, index, "") { }
        public MenuEventArgs(string name) : this(false, 0, name) { }
        public MenuEventArgs(bool value) : this(value, 0, "") { }
    }
}
