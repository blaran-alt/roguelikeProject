using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using Projet.Interfaces;
using RLNET;
using Projet.Items;

namespace Projet.Systems
{
    public class Inventory : IContainer
    {
        public List<Item> Items { get; set; }
        public int Capacity { get;  set; }
        public Selection Selection;

        public Inventory()
        {
            Capacity = 20;
            Items = new List<Item>();
            Selection = new Selection(1, 3, Capacity);
        }

        public void Draw(RLConsole console)
        {
            for (int k = 0; k < console.Width; k++)
            {
                for (int l = 0; l < console.Height; l++)
                {
                    if(k==0 || k==console.Width-1 || l == 0 || l == console.Height - 1)
                    {
                        console.Set(k, l, null, RLColor.Black, null);
                    }
                }
            }
            int width = console.Width - 4;
            int height = console.Height - 5;
            int columnWidth = (int)Math.Ceiling((width * height) / (2f * Capacity));
            Console.WriteLine("Console de taille {0} par {1} donc on aura {2} cells par colonne", width, height, columnWidth);
            int i = 2;
            int j = 3;
            foreach (Item item in Items)
            {
                int offset = (columnWidth - item.Quantity.ToString().Length + 4) / 2;
                item.DrawInContainer(console, i, j);
                i += columnWidth;
                if(i+columnWidth >= width)
                {
                    i = 2;
                    j+=2;
                }
            }
        }

        public bool PickUp(Item item)
        {
            Item _item = Items.Find(i => i.Name == item.Name && i.EffectCode == item.EffectCode);
            if(_item != null)
            {
                _item.Quantity += item.Quantity;
                return true;
            }
            else
            {
                if (Items.Count() < Capacity)
                {
                    Items.Add(item);
                    item.Dropped = false;
                    return true;
                }
            }
            return false;
        }

        public bool Use(string name, int quantity)
        {
            Item item = Items.Find(i => i.Name == name);
            if (item != null)
            {
                if(item.Quantity >= quantity)
                {
                    item.Quantity -= quantity;
                    if (item.Quantity == 0)
                    {
                        Items.Remove(item);
                    }
                    return true;
                }
            }
            return false;
        }
        public bool Use(string name, int effectCode, int quantity)
        {
            Item item = Items.Find(i => i.Name == name && i.EffectCode == effectCode);
            if (item != null)
            {
                if (item.Quantity >= quantity)
                {
                    item.Quantity -= quantity;
                    if (item.Quantity == 0)
                    {
                        Items.Remove(item);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool HasKeys()
        {
            Item[] keys = new Item[3];
            for (int j = 0; j < 3; j++)
            {
                keys[j] = Items.Find(i => i.Name == "Key" && i.EffectCode == j);
                if (keys[j] == null)
                {
                    return false;
                }
            }
            foreach (Item key in keys)
            {
                Items.Remove(key);
            }
            return true;
        }
    }
}
