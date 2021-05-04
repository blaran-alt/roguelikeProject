using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using Projet.Interfaces;
using RLNET;
using Projet.Items;
using RogueSharp;

namespace Projet.Systems
{
    public class Inventory : IContainer
    {
        public List<Item> Items { get; set; }
        public int Capacity { get;  set; }
        public UISelection Selection;

        private List<Item> Potions
        {
            get
            {
                return Items.FindAll(i => i.Name == "Potion");
            }
        }

        public Inventory()
        {
            Capacity = 10;
            Items = new List<Item>();
            Selection = new UISelection(2, 4);
        }

        public void AlternateDraw(RLConsole inventoryConsole, RLConsole mapConsole)
        {
            inventoryConsole.Print(Game.GetPropostionnalSize(25), Game.GetPropostionnalSize(3), "Keys", Colors.Text);
            int j = Game.GetPropostionnalSize(3) + 2;
            foreach (Item item in Items)
            {
                if(item.Name == "Key")
                {
                    item.AlternateDrawInContainer(inventoryConsole, Game.GetPropostionnalSize(25), j);
                    j++;
                }
            }

            inventoryConsole.Print(Game.GetPropostionnalSize(48), Game.GetPropostionnalSize(3), "Potions", Colors.Text);
            CellSelection.CreateRoomWalls(50, 5, 3, 3, 186, 205, 201, 187, 200, 188, inventoryConsole);

            if(Potions.Count() > 0)
            {
                Potion potion = Potions[Selection.AlternateSelectedItemIndex] as Potion;
                potion.AlternateDrawInContainer(inventoryConsole, 51, 6);
                if(potion.EffectCode == 2)
                {
                    CellSelection.HighlightMonstersAround(Game.Player.Coord, 5, mapConsole);
                }
            }
        }

        public void Draw(RLConsole console)
        {
            int width = console.Width - 4;
            int height = console.Height - 5;
            int columnWidth = (width * height) / (2 * Capacity);
            int i = 2;
            int j = 4;
            foreach (Item item in Items)
            {
                int offset = columnWidth / 2 - 2;
                item.DrawInContainer(console, i + offset, j);
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

        public bool Use()
        {
            int index = Selection.SelectedItemIndex;
            if(index < Items.Count())
            {
                Item item = Items[index];
                if (item.Use())
                {
                    item.Quantity--;
                    if(item.Quantity == 0)
                    {
                        Items.Remove(item);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool UsePotion()
        {
            int index = Selection.AlternateSelectedItemIndex;
            if (index < Potions.Count())
            {
                Item item = Potions[index];
                if (item.Use())
                {
                    item.Quantity--;
                    if (item.Quantity == 0)
                    {
                        Items.Remove(item);
                        if (--Selection.AlternateSelectedItemIndex < 0)
                        {
                            Selection.AlternateSelectedItemIndex = 0;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool NextSelection()
        {
            int temp = Selection.AlternateSelectedItemIndex;
            if (++Selection.AlternateSelectedItemIndex >= Potions.Count())
            {
                Selection.AlternateSelectedItemIndex = 0;
            }
            return temp != Selection.AlternateSelectedItemIndex;
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
