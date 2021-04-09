using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RLNET;

namespace Projet.Interfaces
{
    public interface IContainer
    {
        List<Item> Items { get; set; }
        int Capacity { get; set; }
        void Draw(RLConsole console);
    }
}
