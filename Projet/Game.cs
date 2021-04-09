using System;
using RLNET;
using RogueSharp.Random;
using RogueSharp;
using Projet.Core;
using Projet.Systems;
using System.Collections.Generic;
using System.Linq;
using Projet.Items;

namespace Projet
{
    class Game
    {
        // The screen height and width are in number of tiles
        private static readonly int _screenWidth = 100;
        private static readonly int _screenHeight = 70;
        private static RLRootConsole _rootConsole;

        // The map console takes up most of the screen and is where the map will be drawn
        private static readonly int _mapWidth = 80;
        private static readonly int _mapHeight = 48;
        private static RLConsole _mapConsole;

        // Below the map console is the message console which displays attack rolls and other information
        private static readonly int _messageWidth = 80;
        private static readonly int _messageHeight = 11;
        private static RLConsole _messageConsole;

        // The stat console is to the right of the map and display player and monster stats
        private static readonly int _statWidth = 20;
        private static readonly int _statHeight = 70;
        private static RLConsole _statConsole;

        // Above the map is the inventory console which shows the players equipment, abilities, and items
        private static readonly int _inventoryWidth = 80;
        private static readonly int _inventoryHeight = 11;
        private static RLConsole _inventoryConsole;

        private static readonly int _UIWidth = 30;
        private static readonly int _UIHeight = 10;
        private static RLConsole _UIConsole;

        private static readonly int _containerWidth = 70;
        private static readonly int _containerHeight = 30;
        private static RLConsole _containerConsole;

        public static Player Player { get;  set; }
        public static GameMap Map { get; private set; }
        public static CommandSystem CommandSystem { get; private set; }
        public static MessageLog MessageLog { get; private set; }
        public static SchedulingSystem SchedulingSystem { get; private set; }
        public static Inventory Inventory { get; set; }

        //------------------------------------------------------------//
        private static SelectionType _currentSelectionType;
        private static bool _activeSelection;
        private static bool _highlightWalls;
        private static int _selectionSize = 5;
        //------------------------------------------------------------//

        public static IRandom Random { get; private set; }
        private static int seed;

        private static bool _renderRequired = true;
        private static int _mapLevel;
        public static int Level { get { return _mapLevel; } }
        private static bool _gameOver;
        public static bool openInventory = false;

        public static void Main()
        {
            // Establish the seed for the random number generator from the current time
            seed = (int)DateTime.UtcNow.Ticks;
            Random = new DotNetRandom(seed);

            // This must be the exact name of the bitmap font file we are using or it will error.
            string fontFileName = "ascii_8x8.png";
            // The title will appear at the top of the console window
            string consoleTitle = $"RogueSharp V3 Tutorial - Level {_mapLevel} - Seed {seed}";

            // Tell RLNet to use the bitmap font that we specified and that each tile is 8 x 8 pixels
            _rootConsole = new RLRootConsole(fontFileName, _screenWidth, _screenHeight,
              8, 8, 1f, consoleTitle);

            // Initialize the sub consoles that we will Blit to the root console
            _mapConsole = new RLConsole(_mapWidth, _mapHeight);
            _messageConsole = new RLConsole(_messageWidth, _messageHeight);
            _statConsole = new RLConsole(_statWidth, _statHeight);
            _inventoryConsole = new RLConsole(_inventoryWidth, _inventoryHeight);
            _UIConsole = new RLConsole(_UIWidth, _UIHeight);
            _containerConsole = new RLConsole(_containerWidth, _containerHeight);

            // Set up a handler for RLNET's Update event
            _rootConsole.Update += OnRootConsoleUpdate;
            // Set up a handler for RLNET's Render event
            _rootConsole.Render += OnRootConsoleRender;

            // Set background color and text for each console
            _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Colors.Alternate);
            _statConsole.Print(1, 1, "Stats", RLColor.White);

            _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
            _inventoryConsole.Print(1, 1, "Inventory", RLColor.White);

            InitializeGame();
            for (int i = 0; i < 20; i++)
            {
                Inventory.PickUp(new Potion(i));
            }
            // Begin RLNET's game loop
            _rootConsole.Run();
        }

        private static void InitializeGame()
        {
            _mapLevel = 1;

            _rootConsole.Title = $"RogueSharp RLNet Tutorial - Level {_mapLevel} - Seed {seed}";

            CommandSystem = new CommandSystem();
            SchedulingSystem = new SchedulingSystem();
            Inventory = new Inventory();

            MessageLog = new MessageLog();
            MessageLog.Add("The rogue arrives on level 1");
            MessageLog.Add($"Level created with seed '{seed}'");

            MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 30, 13, 7, _mapLevel);
            Map = mapGenerator.CreateMap();
            Map.UpdatePlayerFieldOfView();

            //----------------------------//
            _currentSelectionType = 0;
            _activeSelection = false;
            //----------------------------//

            Player.Reset();

            _gameOver = false;
        }

        // Event handler for RLNET's Update event
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            bool didPlayerAct = false;
            RLKeyPress keyPress = _rootConsole.Keyboard.GetKeyPress();

            if(keyPress != null)
            {
                if(keyPress.Key == RLKey.Escape)
                {
                    if(!_gameOver && openInventory)
                    {
                        openInventory = false;
                        _renderRequired = true;
                    }
                    else
                    {
                        _rootConsole.Close();
                    }
                }
                else if(_gameOver && keyPress.Key == RLKey.Enter)
                {
                    InitializeGame();
                }
            }

            if (!_gameOver)
            {
                if(_rootConsole.Mouse.GetLeftClick())
                {
                    _currentSelectionType++;
                    _renderRequired = true;
                    if ((int)_currentSelectionType == 8)
                    {
                        _currentSelectionType = SelectionType.Radius;
                    }
                }
                //------------------------------------------------------------//
                if(keyPress != null)
                {
                    if (keyPress.Key == RLKey.V)
                    {
                        _activeSelection = !_activeSelection;
                        _renderRequired = true;
                    }
                    if (keyPress.Key == RLKey.C)
                    {
                        _highlightWalls = !_highlightWalls;
                        _renderRequired = true;
                    }
                    else if (keyPress.Key == RLKey.Q)
                    {
                        _selectionSize--;
                        _renderRequired = true;
                        if (_selectionSize == 0)
                        {
                            _selectionSize = 1;
                            _renderRequired = false;
                        }
                    }
                    else if (keyPress.Key == RLKey.E)
                    {
                        _selectionSize++;
                        _renderRequired = true;
                        if (_selectionSize == 51)
                        {
                            _selectionSize = 50;
                            _renderRequired = false;
                        }
                    }
                    else if(keyPress.Key == RLKey.I)
                    {
                        openInventory = !openInventory;
                        _renderRequired = true;
                    }
                }
                if (_activeSelection)
                {
                    _renderRequired = true;
                }
                //------------------------------------------------------------------//

                if (CommandSystem.IsPlayerTurn)
                {
                    if(keyPress != null)
                    {
                        if (keyPress.Key == RLKey.W || keyPress.Key == RLKey.Up)
                        {
                            didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                        }
                        else if (keyPress.Key == RLKey.S || keyPress.Key == RLKey.Down)
                        {
                            didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                        }
                        else if (keyPress.Key == RLKey.A || keyPress.Key == RLKey.Left)
                        {
                            didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                        }
                        else if (keyPress.Key == RLKey.D || keyPress.Key == RLKey.Right)
                        {
                            didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                        }
                        else if (keyPress.Key == RLKey.Period)
                        {
                            if (Map.CanMoveDownToNextLevel())
                            {
                                MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 20, 13, 7, ++_mapLevel);
                                Map = mapGenerator.CreateMap();
                                MessageLog = new MessageLog();
                                CommandSystem = new CommandSystem();
                                _rootConsole.Title = $"RogueSharp RLNet Tutorial - Level {_mapLevel} - Seed {seed}";
                                didPlayerAct = true;
                            }
                        }

                        if (didPlayerAct)
                        {
                            _renderRequired = true;
                            CommandSystem.EndPlayerTurn();
                        }
                    }
                }
                else if(!_gameOver)
                {
                    CommandSystem.ActivateMonsters();
                    _renderRequired = true;
                }
            }
        }

        // Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            if (_renderRequired)
            {
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();
                _inventoryConsole.Clear();
                _containerConsole.Clear();

                _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Colors.Alternate);
                _messageConsole.SetBackColor(0, 0, _messageWidth , _messageHeight, Colors.Secondary);
                _mapConsole.SetBackColor(0, 0, _mapWidth, _mapHeight, Colors.Primary);
                _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
                _inventoryConsole.Print(1, 1, "Inventory", RLColor.White);

                Map.Draw(_mapConsole, _statConsole);
                Player.Draw(_mapConsole, Map);
                MessageLog.Draw(_messageConsole);
                Player.DrawStats(_statConsole);
                Inventory.Draw(_inventoryConsole);

                _statConsole.Print(6, _statHeight - 2, $"({Player.X},{Player.Y})", Colors.Text);

                if (_activeSelection)
                {
                    foreach (ICell cell in SelectCellsAroundMouse())
                    {
                        if (IsInMap(cell) && (_highlightWalls || cell.IsExplored))
                        {
                            _mapConsole.SetBackColor(cell.X, cell.Y, RLColor.Yellow);
                        }
                    }
                }

                // Blit the sub consoles to the root console in the correct locations
                RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _mapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);

                if (openInventory)
                {
                    Inventory.Selection.Reset();
                    _containerConsole.SetBackColor(0, 0, _containerWidth, _containerHeight, Colors.ComplimentDarker);
                    _containerConsole.Print(1, 1, "Inventory", RLColor.White);
                    Inventory.Draw(_containerConsole);
                    Inventory.Selection.Draw(_containerConsole);
                    RLConsole.Blit(_containerConsole, 0, 0, _containerWidth, _containerHeight, _rootConsole, 15, 15);
                }

                // Tell RLNET to draw the console that we set
                _rootConsole.Draw();
                _renderRequired = false;
            }
            else if (_activeSelection)
            {
                _mapConsole.Clear();
                _mapConsole.SetBackColor(0, 0, _mapWidth, _mapHeight, Colors.Primary);
                Map.Draw(_mapConsole, _statConsole);
                Player.Draw(_mapConsole, Map);

                foreach (ICell cell in SelectCellsAroundMouse())
                {
                    if(IsInMap(cell) && (_highlightWalls || cell.IsExplored))
                    {
                        _mapConsole.SetBackColor(cell.X, cell.Y, RLColor.Yellow);
                    }
                }
                RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight, _rootConsole, 0, _inventoryHeight);
                _rootConsole.Draw();
            }
            if (_gameOver)
            {
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();
                _inventoryConsole.Clear();

                Map.Draw(_mapConsole, _statConsole);
                MessageLog.Draw(_messageConsole);
                Player.DrawStats(_statConsole);

                _UIConsole.SetBackColor(0, 0, _UIWidth, _UIHeight, RLColor.Red);
                for (int i = 0; i < _UIWidth; i++)
                {
                    for (int j = 0; j < _UIHeight; j++)
                    {
                        if(i==0 || i==_UIWidth-1 || j == 0 || j == _UIHeight-1)
                        {
                            _UIConsole.SetBackColor(i, j, RLColor.White);
                        }
                    }
                }
                _UIConsole.Print(10, 3, "GAME OVER", RLColor.White);
                _UIConsole.Print(3, 6, "Press Enter to play again", RLColor.LightGray);

                RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _mapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);
                RLConsole.Blit(_UIConsole, 0, 0, _UIWidth, _UIHeight, _rootConsole, 35, 25);

                _rootConsole.Draw();
            }
        }

        private static IEnumerable<ICell> SelectCellsAroundMouse()
        {
            int x = _rootConsole.Mouse.X;
            int y = _rootConsole.Mouse.Y - _inventoryHeight;
            if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
            {
                return new List<ICell>();
            }
            IEnumerable<ICell> selectedCells;
            switch (_currentSelectionType)
            {
                case SelectionType.Radius:
                    {
                        selectedCells = Map.GetCellsInCircle(x, y, _selectionSize);
                        break;
                    }
                case SelectionType.Area:
                    {
                        selectedCells = Map.GetCellsInSquare(x, y, _selectionSize);
                        break;
                    }
                case SelectionType.RadiusBorder:
                    {
                        selectedCells = Map.GetBorderCellsInCircle(x, y, _selectionSize);
                        break;
                    }
                case SelectionType.AreaBorder:
                    {
                        selectedCells = Map.GetBorderCellsInSquare(x, y, _selectionSize);
                        break;
                    }
                case SelectionType.Row:
                    {
                        selectedCells = Map.GetCellsInRows(y);
                        break;
                    }
                case SelectionType.Column:
                    {
                        selectedCells = Map.GetCellsInColumns(x);
                        break;
                    }
                case SelectionType.ColumnAndRow:
                    {
                        List<ICell> rowCells = Map.GetCellsInRows(y).ToList();
                        rowCells.AddRange(Map.GetCellsInColumns(x));
                        selectedCells = rowCells;
                        break;
                    }
                case SelectionType.Cross:
                    {
                        if (x < 1 || x >= _mapWidth - 1 || y < 1 || y >= _mapHeight - 1)
                        {
                            return new List<ICell>();
                        }
                        List<ICell> rowCells = Map.GetCellsInRows(y + 1, y - 1).ToList();
                        rowCells.AddRange(Map.GetCellsInColumns(x + 1, x - 1));
                        selectedCells = rowCells;
                        break;
                    }
                default:
                    {
                        selectedCells = Map.GetCellsInCircle(x, y, _selectionSize);
                        break;
                    }
            }
            if (_highlightWalls)
            {
                return selectedCells;
            }
            return FilterWalls(selectedCells);
        }

        private static IEnumerable<ICell> FilterWalls(IEnumerable<ICell> cells)
        {
            return cells.Where(c => c.IsWalkable);
        }

        private static bool IsInMap(ICell cell)
        {
            return cell.X >= 0 && cell.X < _mapWidth && cell.Y >= 0 && cell.Y < _mapHeight;
        }

        public static void GameOver()
        {
            _gameOver = true;
            _activeSelection = false;
            _renderRequired = true;
        }
    }
}
