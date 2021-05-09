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

        public void DrawWithEffect(RLConsole inventoryConsole, RLConsole mapConsole)
        {
            Draw(inventoryConsole);
            if(Potions.Count() > 0)
            {
                Potion potion = Potions[Selection.SelectedItemIndex] as Potion;
                potion.Draw(inventoryConsole, 48, 6);
                if(potion.EffectCode == 2)
                {
                    CellSelection.HighlightMonstersAround(Game.Player.Coord, 5, mapConsole);
                }
            }
        }

        public void Draw(RLConsole console)
        {
            console.Print(Game.GetProportionnalSizeInContainer(25, console.Width), 3, "Clefs", Colors.Text);
            int j = 5;
            foreach (Item item in Items)
            {
                if (item.Name == "Clef")
                {
                    item.Draw(console, Game.GetProportionnalSizeInContainer(25, console.Width), j);
                    j ++;
                }
            }

            console.Print(45, 3, "Potions", Colors.Text);
            CellSelection.CreateRoomWalls(47, 5, 3, 3, 186, 205, 201, 187, 200, 188, console);
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
                    if (item.Name == "Clef")
                    {
                        Game.Map.UpdateExitState();
                    }
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
            int index = Selection.SelectedItemIndex;
            if (index < Potions.Count())
            {
                Item item = Potions[index];
                if (item.Use())
                {
                    item.Quantity--;
                    if (item.Quantity == 0)
                    {
                        Items.Remove(item);
                        if (--Selection.SelectedItemIndex < 0)
                        {
                            Selection.SelectedItemIndex = 0;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool NextSelection()
        {
            int temp = Selection.SelectedItemIndex;
            if (++Selection.SelectedItemIndex >= Potions.Count())
            {
                Selection.SelectedItemIndex = 0;
            }
            return temp != Selection.SelectedItemIndex;
        }

        public bool PreviousSelection()
        {
            int temp = Selection.SelectedItemIndex;
            if (--Selection.SelectedItemIndex < 0)
            {
                Selection.SelectedItemIndex = Potions.Count - 1;
            }
            return temp != Selection.SelectedItemIndex;
        }

        public bool HasKeys()
        {
            for (int j = 0; j < 3; j++)
            {
                Item key = Items.Find(i => i.Name == "Clef" && i.EffectCode == j);
                if (key == null)
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool UseKeys()
        {
            if (HasKeys())
            {
                foreach (Item key in Items.Where(i => i.Name == "Clef"))
                {
                    Items.Remove(key);
                }
                return true;
            }
            return false;
        }
    }
}
