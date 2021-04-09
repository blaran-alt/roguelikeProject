using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.Interfaces
{
    public interface IItem
    {
        string Name { get; set; }
        int EffectCode { get; set; }
        int Quantity { get; set; }
        bool Dropped { get; set; }
    }
}
