using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.UI
{
    public class MenuEventArgs : EventArgs
    {
        public object Value { get; set; }

        public MenuEventArgs(object value)
        {
            Value = value;
        }
    }
}
