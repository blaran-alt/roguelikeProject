using System;
using RLNET;
using RogueSharp;
using System.Collections.Generic;
using System.Linq;
using Projet.Systems;
using System.Runtime.CompilerServices;

namespace Projet.Core
{
    public enum Direction
    {
        None = 0,
        DownLeft = 1,
        Down = 2,
        DownRight = 3,
        Left = 4,
        Center = 5,
        Right = 6,
        UpLeft = 7,
        Up = 8,
        UpRight = 9
    }

    public class GameMap : Map
    {
        public List<Rectangle> Rooms { get; set; }
        public List<Door> Doors { get; set; }
        private List<Item> Items { get; set; }
        private readonly List<Monster> _monsters;
        public List<Box> Boxs { get; set; }

        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }

        public bool LightsOn { get; protected set; }
        public List<Connection> Connections { get; set; }
        public int nbConnections = 5;

        public GameMap()
        {
            Game.SchedulingSystem.Clear();
            // Initialize the list of rooms when we create a new DungeonMap
            Rooms = new List<Rectangle>();
            _monsters = new List<Monster>();
            Doors = new List<Door>();
            Items = new List<Item>();
            Boxs = new List<Box>();
            Connections = new List<Connection>();
            LightsOn = true;
        }

        // The Draw method will be called each time the map is updated
        // It will render all of the symbols/colors for each cell to the map sub console
        public virtual void Draw(RLConsole mapConsole, RLConsole statConsole, bool NextAnimation)
        {
            // Sets the right symbol for every cell of the map
            foreach (Cell cell in GetAllCells())
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }
            // Places the doors
            foreach (Door door in Doors)
            {
                door.Draw(mapConsole, this, false);
            }
            // Draws the 2 stairs in the map
            StairsUp.Draw(mapConsole, this, false);
            StairsDown.Draw(mapConsole, this, false);
            foreach (Item item in Items)
            {
                item.Draw(mapConsole, this, false);
            }
            //Draw the terminals
            foreach (Connection connection in Connections)
            {
                connection.TerminalA.Draw(mapConsole, this, false);
                connection.TerminalB.Draw(mapConsole, this, false);
            }

            // Places the monsters after the doors,stairs and items so they appear above them
            int i = 0;
            foreach (Monster monster in _monsters)
            {
                monster.Draw(mapConsole, this, NextAnimation);
                if (IsInFov(monster.X, monster.Y))
                {
                    monster.DrawStats(statConsole, i);
                    i++;
                }
            }

            foreach (Box box in Boxs)
            {
                box.Draw(mapConsole, this, false);
            }
        }

        protected void SetConsoleSymbolForCell(RLConsole console, Cell cell)
        {
            // When we haven't explored a cell yet, we don't want to draw anything
            if (!cell.IsExplored)
            {
                return;
            }

            // When a cell is currently in the field-of-view it should be drawn with ligher colors
            if (IsInFov(cell.X, cell.Y))
            {
                // Choose the symbol to draw based on if the cell is walkable or not
                // '.' for floor and '#' for walls
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, '.');

                    //if (Game.mapGenerator is CortexGenerator generator && generator.HotPath.Any(c => c.X == cell.X && c.Y == cell.Y))
                    //{
                    //    console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.Primary, '.');
                    //    if (generator.startToEndPath.Steps.Any(c => c.X == cell.X && c.Y == cell.Y))
                    //    {
                    //        console.Set(cell.X, cell.Y, Colors.FloorFov, RLColor.Red, '.');
                    //    }
                    //}

                    //console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, ' ' );
                    //console.Print(cell.X, cell.Y, dijkstraIndeces[cell.X, cell.Y].ToString(), Colors.FloorFov);
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, '#');

                    //if (Game.mapGenerator is CortexGenerator generator && generator.HotPath.Any(c => c.X == cell.X && c.Y == cell.Y))
                    //{
                    //    console.Set(cell.X, cell.Y, Colors.WallFov, Colors.Primary, '#');
                    //    if (generator.startToEndPath.Steps.Any(c => c.X == cell.X && c.Y == cell.Y))
                    //    {
                    //        console.Set(cell.X, cell.Y, Colors.FloorFov, RLColor.Red, '.');
                    //    }
                    //}

                    //console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, ' ');
                    //console.Print(cell.X, cell.Y, dijkstraIndeces[cell.X, cell.Y].ToString(), Colors.WallFov);
                }
            }
            // When a cell is outside of the field of view draw it with darker colors
            else if(LightsOn)
            {
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.Floor, Colors.FloorBackground, '.');

                    //console.Set(cell.X, cell.Y, Colors.Floor, Colors.FloorBackground, ' ');
                    //console.Print(cell.X, cell.Y, dijkstraIndeces[cell.X, cell.Y].ToString(), Colors.Floor);
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, '#');

                    //console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, ' ');
                    //console.Print(cell.X, cell.Y, dijkstraIndeces[cell.X, cell.Y].ToString(), Colors.Wall);
                }
            }
        }

        public void UpdatePlayerFieldOfView()
        {
            Player player = Game.Player;
            // Compute the field-of-view based on the player's location and awareness
            int playerAwareness = player.Awareness;
            if(!LightsOn)
            {
                playerAwareness -= 7;
            }
            ComputeFov(player.X, player.Y, playerAwareness, true);
            // Mark all cells in field-of-view as having been explored
            foreach (Cell cell in GetAllCells())
            {
                if (IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }

        public bool SetActorPosition(Actor actor, int x, int y)
        {
            // Only allow actor placement if the cell is walkable
            if (IsInMap(x,y) && GetCell(x, y).IsWalkable)
            {
                // The cell the actor was previously on is now walkable
                SetIsWalkable(actor.X, actor.Y, true);
                // Update the actor's position
                actor.X = x;
                actor.Y = y;
                // The new cell the actor is on is now not walkable
                SetIsWalkable(actor.X, actor.Y, false);
                // Try open a door if one exists here
                OpenDoor(actor, x, y);
                // Don't forget to update the field of view if we just repositioned the player
                if (actor is Player)
                {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }
            return false;
        }

        public bool SetActorPosition(Box actor, int x, int y)
        {
            // Only allow actor placement if the cell is walkable
            if (GetCell(x, y).IsWalkable)
            {
                // The cell the actor was previously on is now walkable
                SetIsWalkable(actor.X, actor.Y, true);
                // Update the actor's position
                actor.X = x;
                actor.Y = y;
                // The new cell the actor is on is now not walkable
                SetIsWalkable(actor.X, actor.Y, false);
                return true;
            }
            return false;
        }

        // A helper method for setting the IsWalkable property on a Cell
        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            ICell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
            return;
        }
        public void SetIsWalkable(Point coord, bool isWalkable)
        {
            ICell cell = GetCell(coord.X, coord.Y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
            return;
        }

        public void AddPlayer(Player player)
        {
            Game.Player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();
            Game.SchedulingSystem.Add(player);
        }

        public void AddMonster(Monster monster)
        {
            _monsters.Add(monster);
            // After adding the monster to the map make sure to make the cell not walkable
            SetIsWalkable(monster.X, monster.Y, false);
            Game.SchedulingSystem.Add(monster);
        }
        public void RemoveMonster(Monster monster)
        {
            _monsters.Remove(monster);
            // After removing the monster from the map, make sure the cell is walkable again
            SetIsWalkable(monster.X, monster.Y, true);
            Game.SchedulingSystem.Remove(monster);
        }
        public Monster GetMonsterAt(int x, int y)
        {
            return _monsters.FirstOrDefault(m => m.X == x && m.Y == y);
        }

        public void AddItem(Item item)
        {
            Item check = GetItemAt(item.X, item.Y);
            if (check != null)
            {
                if(check.IsEqual(item))
                {
                    check.Quantity += item.Quantity;
                }
                else
                {
                    if (IsWalkable(item.X + 1, item.Y))
                    {
                        item.X++;
                        AddItem(item);
                    }
                    else if(IsWalkable(item.X - 1, item.Y))
                    {
                        item.X--;
                        AddItem(item);
                    }
                    else if (IsWalkable(item.X, item.Y + 1))
                    {
                        item.Y++;
                        AddItem(item);
                    }
                    else if (IsWalkable(item.X, item.Y - 1))
                    {
                        item.Y--;
                        AddItem(item);
                    }
                    else
                    {
                        Point newLocation = GetRandomWalkableLocationInRoom(new Rectangle(item.X - 3, item.Y - 3, item.X + 3, item.Y + 3));
                        item.X = newLocation.X;
                        item.Y = newLocation.Y;
                        AddItem(item);
                    }
                }
            }
            else
            {
                Items.Add(item);
            }
        }
        public void RemoveItem(Item item)
        {
            Items.Remove(item);
        }
        public Item GetItemAt(int x, int y)
        {
            return Items.FirstOrDefault(i => i.X == x && i.Y == y);
        }

        public void AddBox(Box box)
        {
            Boxs.Add(box);
            SetIsWalkable(box.X, box.Y, false);
        }
        public Box GetBoxAt(Point coord)
        {
            return Boxs.FirstOrDefault(b => b.Coord == coord);
        }
        public Box GetBoxAt(int x, int y)
        {
            return Boxs.FirstOrDefault(b => b.X == x && b.Y == y);
        }

        public void AddTerminals(Connection connection)
        {
            Connections.Add(connection);
            SetIsWalkable(connection.TerminalA.X, connection.TerminalA.Y, false);
            SetIsWalkable(connection.TerminalB.X, connection.TerminalB.Y, false);
        }
        public Terminal GetTerminalAt(Point coord)
        {
            Terminal terminal = Connections.FirstOrDefault(c => c.TerminalA.Coord == coord).TerminalA;
            if(terminal == null)
            {
                terminal = Connections.FirstOrDefault(c => c.TerminalB.Coord == coord).TerminalB;
            }
            return terminal;
        }
        public Terminal GetTerminalAt(int x, int y)
        {
            Connection connection = Connections.FirstOrDefault(c => (c.TerminalA.X == x && c.TerminalA.Y == y) || (c.TerminalB.X == x && c.TerminalB.Y == y));
            if(connection != null)
            {
                Terminal terminal = connection.TerminalA;
                if(terminal.X != x || terminal.Y != y)
                {
                    terminal = connection.TerminalB;
                }
                return terminal;
            }
            return null;
        }

        // Look for a random location in the room that is walkable.
        public Point GetRandomWalkableLocationInRoom(Rectangle room)
        {
            if (DoesRoomHaveWalkableSpace(room))
            {
                for (int i = 0; i < 100; i++)
                {
                    int x = Game.Random.Next(1, room.Width - 2) + room.X;
                    int y = Game.Random.Next(1, room.Height - 2) + room.Y;
                    if (IsWalkable(x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }

            // If we didn't find a walkable location in the room return zero
            return Point.Zero;
        }

        // Iterate through each Cell in the room and return true if any are walkable
        public bool DoesRoomHaveWalkableSpace(Rectangle room)
        {
            for (int x = 1; x <= room.Width - 2; x++)
            {
                for (int y = 1; y <= room.Height - 2; y++)
                {
                    if (IsWalkable(x + room.X, y + room.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Door GetDoor(int x, int y)
        {
            return Doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }

        // The actor opens the door located at the x,y position
        private void OpenDoor(Actor actor, int x, int y)
        {
            Door door = GetDoor(x, y);
            if (door != null && !door.IsOpen)
            {
                door.IsOpen = true;
                var cell = GetCell(x, y);
                // Once the door is opened it should be marked as transparent and no longer block field-of-view
                SetCellProperties(x, y, true, cell.IsWalkable, cell.IsExplored);

                Game.MessageLog.Add($"{actor.Name} opened a door");
            }
        }

        public bool CanMoveDownToNextLevel()
        {
            Player player = Game.Player;
            return StairsDown.X == player.X && StairsDown.Y == player.Y && NextLevelCondition();
        }

        public virtual bool NextLevelCondition()
        {
            return Game.Inventory.HasKeys();
        }

        public bool IsInMap(ICell cell)
        {
            return IsInMap(cell.X, cell.Y);
        }
        public bool IsInMap(Point cell)
        {
            return IsInMap(cell.X, cell.Y);
        }
        public bool IsInMap(int x,int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
