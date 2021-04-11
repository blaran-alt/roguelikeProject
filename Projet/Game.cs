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
        private static readonly int _onConsoleMapWidth = 80;
        private static readonly int _onConsoleMapHeight = 48;
        private static readonly int _mapWidth = 120;
        private static readonly int _mapHeight = 60;
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
        private static readonly int _containerHeight = 24;
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
        private static bool openInventory = false;
        private static Point lastMousePos;

        public static void Main()
        {

            // Establish the seed for the random number generator from the current time
            seed = (int)DateTime.UtcNow.Ticks;
            Random = new DotNetRandom(seed);

            // This must be the exact name of the bitmap font file we are using or it will error.
            string fontFileName = "GraphicTest.png";
            // The title will appear at the top of the console window
            string consoleTitle = $"RogueSharp V3 Tutorial - Level {_mapLevel} - Seed {seed}";

            // Tell RLNet to use the bitmap font that we specified and that each tile is 8 x 8 pixels
            _rootConsole = new RLRootConsole(fontFileName, _screenWidth, _screenHeight,
              16, 16, .6f, consoleTitle);

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
            _rootConsole.SetBackColor(0, _inventoryHeight, _onConsoleMapWidth, _onConsoleMapHeight, Colors.Primary);
            _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Colors.Alternate);
            _statConsole.Print(1, 1, "Stats", RLColor.White);

            _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
            _inventoryConsole.Print(1, 1, "Inventory", RLColor.White);

            InitializeGame();

            // Begin RLNET's game loop
            _rootConsole.Run();
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private static Point GetMapBlitOrigin()
        {
            int x = Clamp(Player.X - _onConsoleMapWidth / 2, 0 ,_mapWidth - _onConsoleMapWidth);
            int y = Clamp(Player.Y - _onConsoleMapHeight / 2, 0,_mapHeight - _onConsoleMapHeight);
            return new Point(x, y);
        }

        public static Point GetMapToConsoleCoord(Point point)
        {
            return point + GetMapBlitOrigin() - new Point(0, _inventoryHeight);
        }

        public static Point GetMousePosOnMap()
        {
            return new Point(_rootConsole.Mouse.X, _rootConsole.Mouse.Y) + GetMapBlitOrigin() - new Point(0, _inventoryHeight);
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

            MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 50, 20, 7, _mapLevel);
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
            bool selectionChanged = false;
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
                else if(keyPress.Key == RLKey.Enter)
                {
                    if (_gameOver)
                    {
                        InitializeGame();
                    }
                    else if (openInventory)
                    {
                        if (Inventory.Use())
                        {
                            _renderRequired = true;
                            openInventory = false;
                        }
                    }
                }
                else if(keyPress.Key == RLKey.H)
                {
                    CellSelection.StartShochWaveEffect(Player.Coord, 20);
                    _renderRequired = true;
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
                    if (keyPress.Key == RLKey.V && !openInventory)
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
                        if (_activeSelection)
                        {
                            _selectionSize--;
                            _renderRequired = true;
                            if (_selectionSize == 0)
                            {
                                _selectionSize = 1;
                                _renderRequired = false;
                            }
                        }
                        else
                        {
                            if (Inventory.UsePotion())
                            {
                                _renderRequired = true;
                            }
                        }
                    }
                    else if (keyPress.Key == RLKey.E)
                    {
                        if (_activeSelection)
                        {
                            _selectionSize++;
                            _renderRequired = true;
                            if (_selectionSize == 51)
                            {
                                _selectionSize = 50;
                                _renderRequired = false;
                            }
                        }
                        else
                        {
                            if (Inventory.NextSelection())
                            {
                                _renderRequired = true;
                            }
                        }
                    }
                    else if(keyPress.Key == RLKey.I)
                    {
                        openInventory = !openInventory;
                        if (openInventory)
                        {
                            Inventory.Selection.Reset();
                            _activeSelection = false;
                        }
                        _renderRequired = true;
                    }
                }
                if (_activeSelection)
                {
                    RLMouse mouse = _rootConsole.Mouse;
                    Point mousePos = new Point(mouse.X, mouse.Y);
                    if (mousePos != lastMousePos)
                    {
                        _renderRequired = true;
                    }
                    lastMousePos = mousePos;
                }
                //------------------------------------------------------------------//

                if (CommandSystem.IsPlayerTurn)
                {
                    if(keyPress != null)
                    {
                        if (keyPress.Key == RLKey.W || keyPress.Key == RLKey.Up)
                        {
                            if (openInventory)
                            {
                                selectionChanged = Inventory.Selection.MoveSelection(Direction.Up, Inventory.Items.Count());
                            }
                            else
                            {
                                didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                            }
                        }
                        else if (keyPress.Key == RLKey.S || keyPress.Key == RLKey.Down)
                        {
                            if (openInventory)
                            {
                                selectionChanged = Inventory.Selection.MoveSelection(Direction.Down, Inventory.Items.Count());
                            }
                            else
                            {
                                didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                            }
                        }
                        else if (keyPress.Key == RLKey.A || keyPress.Key == RLKey.Left)
                        {
                            if (!openInventory)
                            {
                                didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                            }

                        }
                        else if (keyPress.Key == RLKey.D || keyPress.Key == RLKey.Right)
                        {
                            if (!openInventory)
                            {
                                didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                            }
                        }
                        else if (keyPress.Key == RLKey.Period && !openInventory)
                        {
                            if (Map.CanMoveDownToNextLevel())
                            {
                                MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 50, 20, 7, ++_mapLevel);
                                Map = mapGenerator.CreateMap();
                                MessageLog = new MessageLog();
                                CommandSystem = new CommandSystem();
                                _rootConsole.Title = $"RogueSharp RLNet Tutorial - Level {_mapLevel} - Seed {seed}";
                                MessageLog.Add($"The rogue arrives on level {_mapLevel}");
                                MessageLog.Add($"Level created with seed '{seed}'");
                                didPlayerAct = true;
                            }
                        }

                        if (didPlayerAct)
                        {
                            _renderRequired = true;
                            CommandSystem.EndPlayerTurn();
                        }
                        if (selectionChanged)
                        {
                            _renderRequired = true;
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
                _rootConsole.Clear();
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();
                _inventoryConsole.Clear();
                _containerConsole.Clear();

                _rootConsole.SetBackColor(0, _inventoryHeight, _onConsoleMapWidth, _onConsoleMapHeight, Colors.Primary);
                _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Colors.Alternate);
                _messageConsole.SetBackColor(0, 0, _messageWidth , _messageHeight, Colors.Secondary);
                _mapConsole.SetBackColor(0, 0, _mapWidth, _mapHeight, Colors.Primary);
                _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
                _inventoryConsole.Print(1, 1, "Inventory", RLColor.White);

                Map.Draw(_mapConsole, _statConsole);
                Player.Draw(_mapConsole, Map);
                MessageLog.Draw(_messageConsole);
                Player.DrawStats(_statConsole);
                _statConsole.Print(_statWidth + 3, _statHeight - 3, $"({Player.X},{Player.Y})",Colors.Text);
                Inventory.AlternateDraw(_inventoryConsole, _mapConsole);

                _statConsole.Print(6, _statHeight - 2, $"({Player.X},{Player.Y})", Colors.Text);

                if (_activeSelection)
                {
                    CellSelection.DrawCellsAroudPoint(GetMousePosOnMap(), _currentSelectionType, _selectionSize, _highlightWalls, _mapConsole);
                }

                _renderRequired = CellSelection.ShockWaveEffect( _mapConsole);

                // Blit the sub consoles to the root console in the correct locations
                Point mapBlitOrigin = GetMapBlitOrigin();
                RLConsole.Blit(_mapConsole, mapBlitOrigin.X, mapBlitOrigin.Y, _onConsoleMapWidth, _onConsoleMapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _onConsoleMapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);

                if (openInventory)
                {
                    _containerConsole.Clear();
                    _containerConsole.SetBackColor(0, 0, _containerWidth, _containerHeight, Colors.ComplimentDarker);
                    _containerConsole.Print(1, 1, "Inventory", RLColor.White);
                    Inventory.Draw(_containerConsole);
                    Inventory.Selection.Draw(_containerConsole);
                    RLConsole.Blit(_containerConsole, 0, 0, _containerWidth, _containerHeight, _rootConsole, 15, 15);
                }

                // Tell RLNET to draw the console that we set
                _rootConsole.Draw();
            }
            if (_gameOver)
            {
                _rootConsole.Clear();
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

                Point mapBlitOrigin = GetMapBlitOrigin();
                RLConsole.Blit(_mapConsole, mapBlitOrigin.X, mapBlitOrigin.Y, _onConsoleMapWidth, _onConsoleMapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _onConsoleMapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);
                RLConsole.Blit(_UIConsole, 0, 0, _UIWidth, _UIHeight, _rootConsole, 35, 25);

                _rootConsole.Draw();
            }
        }

        public static void GameOver()
        {
            _gameOver = true;
            _activeSelection = false;
            _renderRequired = true;
        }
    }
}
