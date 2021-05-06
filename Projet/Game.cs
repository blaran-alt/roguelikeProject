using System;
using RLNET;
using RogueSharp.Random;
using RogueSharp;
using Projet.Core;
using Projet.Systems;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Projet.Items;
using Projet.UI;
using System.Net;
using System.Threading;
using RandomAccessPerlinNoise;
using RogueSharp.DiceNotation;

namespace Projet
{
    class Game
    {
        // The screen height and width are in number of tiles
        private static readonly int _screenWidth = 85;
        private static readonly double SizeRatio = _screenWidth / 100f;
        private static readonly int _screenHeight = GetProportionnalHorizontalSize(55);
        private static RLRootConsole _rootConsole;

        public static int GetProportionnalHorizontalSize(int value)
        {
            return Math.Max((int)Math.Round(value * SizeRatio), 1);
        }
        public static int GetProportionnalVerticalSize(int value)
        {
            return Math.Max((int)Math.Round(value * _screenHeight / 100f), 1);
        }
        public static Point GetProportionnalSize(int x, int y)
        {
            return new Point(GetProportionnalHorizontalSize(x),GetProportionnalVerticalSize(y));
        }

        // The map console takes up most of the screen and is where the map will be drawn
        private static readonly int _onConsoleMapWidth = GetProportionnalHorizontalSize(80);
        private static readonly int _onConsoleMapHeight = GetProportionnalHorizontalSize(40);
        private static readonly int _mapWidth = 120;
        private static readonly int _mapHeight = 60;
        private static RLConsole _mapConsole;

        // Below the map console is the message console which displays attack rolls and other information
        private static readonly int _messageWidth = GetProportionnalHorizontalSize(80);
        private static readonly int _messageHeight = GetProportionnalHorizontalSize(8);
        private static RLConsole _messageConsole;

        // The stat console is to the right of the map and display player and monster stats
        private static readonly int _statWidth = GetProportionnalHorizontalSize(20);
        private static readonly int _statHeight = _screenHeight;
        private static RLConsole _statConsole;

        // Above the map is the inventory console which shows the players equipment, abilities, and items
        private static readonly int _inventoryWidth = GetProportionnalHorizontalSize(80);
        private static readonly int _inventoryHeight = GetProportionnalHorizontalSize(7);
        private static RLConsole _inventoryConsole;

        private static readonly int _UIWidth = GetProportionnalHorizontalSize(40);
        private static readonly int _UIHeight = GetProportionnalHorizontalSize(10);
        private static RLConsole _UIConsole;

        private static readonly int _containerWidth = GetProportionnalHorizontalSize(70);
        private static readonly int _containerHeight = GetProportionnalHorizontalSize(24);
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
        private static int _seed;

        private static Save _save;

        private static bool _renderRequired = true;
        private static int _mapLevel;
        public static int Level { get { return _mapLevel; } }
        private static bool _gameOver;
        private static bool openInventory = false;
        private static Point lastMousePos;

        private static Menu menu;
        private static bool displayMenu;

        private static bool playWithSeed;

        private static readonly string url = "http://dreamlo.com/lb/";
        private static readonly string privateCode = "B74mhUF1P0qYbdl25ck9SwvmHyT-sZtU2paBghAtcHtw";
        private static readonly string publicCode = "607d5d13778d3cf9c4183991";
        private static readonly WebClient client;

        public static void Main()
        {
            _gameOver = true;

            menu = new MainMenu(_screenWidth, _screenHeight);
            displayMenu = true;

            //client = new WebClient();
            //client.UploadString(url + privateCode + "/add" + "/Name3/91","");
            //string leaderBoard = client.DownloadString(url + publicCode + "/pipe");
            //string[] scores = leaderBoard.Split(new char[] {'\n'},System.StringSplitOptions.RemoveEmptyEntries);
            //foreach (string score in scores)
            //{
            //    string[] info = score.Split(new char[] { '|' });
            //    Console.WriteLine($"Username : {info[0]} - score = {info[1]}");
            //}
            //Console.WriteLine(leaderBoard);

            //save = new Save("Test");
            //seed = save.Seeds[0];


            // This must be the exact name of the bitmap font file we are using or it will error.
            string fontFileName = "ExtendTestBis.png";

            Console.WriteLine(SizeRatio);
            // Tell RLNet to use the bitmap font that we specified and that each tile is 8 x 8 pixels
            _rootConsole = new RLRootConsole(fontFileName, _screenWidth, _screenHeight,
              16, 16, 1f, "Game Name");
            _rootConsole.SetWindowState(RLWindowState.Fullscreen);

            // Initialize the sub consoles that we will Blit to the root console
            _mapConsole = new RLConsole(_mapWidth, _mapHeight); // Console that displays the map
            _messageConsole = new RLConsole(_messageWidth, _messageHeight); // Console that displays the messages
            _statConsole = new RLConsole(_statWidth, _statHeight);// Console that displays the stats
            _inventoryConsole = new RLConsole(_inventoryWidth, _inventoryHeight);// Console that displays the inventory
            _UIConsole = new RLConsole(_screenWidth, _screenHeight);// Console that displays the menu and gameOver message
            _containerConsole = new RLConsole(_containerWidth, _containerHeight);// Console that displays the content of any container (for now just the opened inventory)

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

            _time = DateTime.Now;

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
            return GetMapToConsoleCoord(GetMousePos());
        }
        public static Point GetMousePos()
        {
            return new Point(_rootConsole.Mouse.X, _rootConsole.Mouse.Y);
        }
        public static MapGenerator mapGenerator;

        private static void InitializeGame()
        {
            if (playWithSeed)
            {
                _seed = _save.Seeds[_mapLevel - 1];
            }
            else
            {
                // Establish the seed for the random number generator from the current time
                _seed = (int)DateTime.UtcNow.Ticks;

                _save = new Save();
                _save.Seeds.Add(_seed);
            }
            Random = new DotNetRandom(_seed);
            DisplayAsync();

            _rootConsole.Title = $"RogueSharp RLNet Tutorial - Level {_mapLevel} - Seed {_seed}";

            CommandSystem = new CommandSystem();
            SchedulingSystem = new SchedulingSystem();
            Inventory = new Inventory();

            MessageLog = new MessageLog();
            MessageLog.Add("The rogue arrives on level " + _mapLevel);
            MessageLog.Add($"Level created with seed '{_seed}'");

            if(_mapLevel == 1)
            {
                mapGenerator = new CortexGenerator(_mapWidth, _mapHeight, 30, 10, 7, _mapLevel);
            }
            else
            {
                mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 30, 10, 7, _mapLevel);
            }

            Map = mapGenerator.CreateMap(_seed);
            Map.UpdatePlayerFieldOfView();

            //----------------------------//
            _currentSelectionType = 0;
            _activeSelection = false;
            //----------------------------//

            Player.Reset();

            _gameOver = false;
        }

        private static DateTime _time;
        private static TextArea inputText;
        private static bool takeTextInput = false;

        // Event handler for RLNET's Update event
        private static void OnRootConsoleUpdate(object sender, UpdateEventArgs e)
        {
            bool didPlayerAct = false;
            bool selectionChanged = false;
            RLKeyPress keyPress = _rootConsole.Keyboard.GetKeyPress();

            if (_rootConsole.Mouse.GetLeftClick())
            {
                inputText = null;
                takeTextInput = false;
                if (menu.Click())
                {
                    _renderRequired = true;
                }
            }

            if (keyPress != null)
            {
                if (takeTextInput)
                {
                    if(inputText.Value == inputText.DefaultValue)
                    {
                        inputText.Value = "";
                    }
                    if (keyPress.Key == RLKey.Space)
                    {
                        inputText.Value += ' ';
                        _renderRequired = true;
                    }
                    else if(keyPress.Key == RLKey.BackSpace)
                    {
                        if(inputText.Value.Length > 0)
                        {
                            inputText.Value = inputText.Value.Remove(inputText.Value.Length - 1);
                        }
                        _renderRequired = true;
                    }
                    else if(keyPress.Key == RLKey.Enter)
                    {
                        inputText.EnterPressed = true;
                        inputText.Click();
                    }
                    else if(keyPress.Key.ToString().Length == 7 && keyPress.Key.ToString().Substring(0,6) == "Number")
                    {
                        char inputChar = keyPress.Key.ToString()[6];
                        Console.WriteLine(inputChar);
                        inputText.Value += inputChar;
                    }
                    if (inputText.Value == "")
                    {
                        inputText.Value = inputText.DefaultValue;
                    }
                }
                else
                {
                    if (keyPress.Key == RLKey.Escape)
                    {
                        if (!_gameOver && openInventory)
                        {
                            openInventory = false;
                            _renderRequired = true;
                        }
                        else
                        {
                            _rootConsole.Close();
                        }
                    }
                    else if (keyPress.Key == RLKey.Enter)
                    {
                        if (_gameOver)
                        {
                            if (!playWithSeed)
                            {
                                _save.SaveInFile("oui");
                            }
                            _mapLevel = 1;
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
                    else if (keyPress.Key == RLKey.H)
                    {
                        CellSelection.StartShochWaveEffect(Player.Coord, 20);
                        _renderRequired = true;
                    }
                }
            }

            if (!_gameOver)
            {
                if(_rootConsole.Mouse.GetLeftClick())
                {
                    if (_activeSelection)
                    {
                        _currentSelectionType++;
                        _renderRequired = true;
                        if ((int)_currentSelectionType == 8)
                        {
                            _currentSelectionType = SelectionType.Radius;
                        }
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
                            //if (Inventory.NextSelection())
                            //{
                            //    _renderRequired = true;
                            //}
                            foreach(ICell cell in Map.GetBorderCellsInCircle(Player.X, Player.Y, 1))
                            {
                                Terminal terminal = Map.GetTerminalAt(cell.X, cell.Y);
                                if(terminal != null)
                                {
                                    terminal.isActive = !terminal.isActive;
                                    Map.StairsDown.IsOpen = Map.NextLevelCondition();
                                    _renderRequired = true;
                                    break;
                                }
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
                                Thread thread = new Thread(LoadNextLevel);
                                thread.Start();

                                DisplayAsync();

                                thread.Join();

                                MessageLog.Add($"The rogue arrives on level {_mapLevel}");
                                MessageLog.Add($"Level created with seed '{_seed}'");
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

        public static int GetCenterOffset(int width, int length)
        {
            return (width - length) / 2;
        }
        public static int GetCenterVerticalOffset(int containerHeight, int inHeight)
        {
            return (containerHeight - inHeight) / 2;
        }
        public static int GetEvenlySpacedOffset(int height, int linesNb)
        {
            return (int)Math.Floor(((decimal)height - linesNb) / (linesNb + 1));
        }

        private static void LoadNextLevel()
        {
            if (playWithSeed)
            {
                _seed = _save.Seeds[_mapLevel - 1];
            }
            else
            {
                // Creating a new seed for the next level
                _seed = (int)DateTime.UtcNow.Ticks;
                // Saving that seed int the save
                _save.Seeds.Add(_seed);
            }

            Random = new DotNetRandom(_seed);
            mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 50, 20, 7, ++_mapLevel);
            Map = mapGenerator.CreateMap(_seed);
            MessageLog = new MessageLog();
            CommandSystem = new CommandSystem();
        }

        private static void DisplayAsync()
        {
            string text = Story.TransitionTexts[_mapLevel - 1];
            _UIConsole.Clear();
            _UIConsole.SetBackColor(0, 0, _UIConsole.Width, _UIConsole.Height, RLColor.Black);
            _UIConsole.SetChar(1, 1, '_');
            int i = 1;
            int j = 1;
            foreach (char letter in text)
            {
                _UIConsole.SetChar(i, j, letter);
                _UIConsole.SetChar(i+1, j, '_');
                RLConsole.Blit(_UIConsole, 0, 0, _UIConsole.Width, _UIConsole.Height, _rootConsole, 0, 0);
                _rootConsole.Draw();
                if (letter == '.')
                {
                    Thread.Sleep(500);
                }
                Thread.Sleep(Dice.Roll("3D30"));
                i++;
                if (i >= _UIConsole.Width - 3)
                {
                    j += 2;
                    i = 1;
                }
            }
            Thread.Sleep(2000);
        }

        public static void Quit(object sender, EventArgs args)
        {
            _rootConsole.Close();
        }

        public static void Start(object sender, EventArgs args)
        {
            Console.WriteLine("Start !");
            playWithSeed = false;
            displayMenu = false;
            _mapLevel = 1;
            InitializeGame();
        }

        public static void OpenSeedMenu(object sender, EventArgs args)
        {
            menu = new ChoseSeedMenu(_screenWidth, _screenHeight);
        }

        public static void OpenSaveMenu(Save save)
        {
            menu = new ChoseLevel(_screenWidth, _screenHeight, _save);
        }

        public static void TakeInput(object sender, EventArgs args)
        {
            takeTextInput = true;
            inputText = (TextArea)sender;
        }

        public static void StartWithSeed(int seed)
        {
            playWithSeed = true;
            _seed = seed;
            if(_save.Seeds.Count > 1)
            {
                _mapLevel = _save.Seeds.FindIndex(s => s == seed) + 1;
                if(_mapLevel == -1)
                {
                    _mapLevel = 1;
                }
            }
            else
            {
                _mapLevel = 1;
            }
            InitializeGame();
            displayMenu = false;
            takeTextInput = false;
        }

        private static bool _nextAnimation;

        // Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            DateTime _currentTime = DateTime.Now;
            if (!_gameOver && _time.AddMilliseconds(160) < _currentTime)
            {
                _renderRequired = true;
                _nextAnimation = true;
                _time = _currentTime;
            }
            if (_renderRequired && !displayMenu && !_draw)
            {
                _rootConsole.Clear();
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();
                _inventoryConsole.Clear();
                _containerConsole.Clear();

                Point mapBlitOrigin = GetMapBlitOrigin();

                _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Colors.Alternate);
                _messageConsole.SetBackColor(0, 0, _messageWidth , _messageHeight, Colors.Secondary);
                if(_mapLevel == 1)
                {
                    _mapConsole.SetBackColor(mapBlitOrigin.X, mapBlitOrigin.Y, _onConsoleMapWidth, _onConsoleMapHeight, RLColor.Black);
                }
                else
                {
                    for (int i = 0; i < _onConsoleMapHeight; i++)
                    {
                        _mapConsole.SetBackColor(mapBlitOrigin.X, mapBlitOrigin.Y + i, _onConsoleMapWidth, 1, RLColor.Blend(Colors.gradient1, Colors.gradient2, 1f - i / (_onConsoleMapHeight - 1f)));
                    }
                }
                //_mapConsole.SetBackColor(0, 0, _mapWidth, _mapHeight, Colors.Primary);
                _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
                _inventoryConsole.Print(1, 1, "Inventory", RLColor.White);

                Map.Draw(_mapConsole, _statConsole, _nextAnimation);
                Player.Draw(_mapConsole, Map, _nextAnimation);
                MessageLog.Draw(_messageConsole);
                Player.DrawStats(_statConsole);
                _statConsole.Print(_statWidth + 3, _statHeight - 3, $"({Player.X},{Player.Y})",Colors.Text);
                Inventory.AlternateDraw(_inventoryConsole, _mapConsole);

                _statConsole.Print(6, _statHeight - 2, $"({Player.X},{Player.Y})", Colors.Text);
                ICell cell = Map.GetCell(Player.X, Player.Y);
                _statConsole.Print(0, _statHeight - 1, $"T: {cell.IsTransparent}, W: {cell.IsWalkable}", Colors.Text);

                if (_activeSelection)
                {
                    CellSelection.DrawCellsAroudPoint(GetMousePosOnMap(), _currentSelectionType, _selectionSize, _highlightWalls, _mapConsole);
                }

                _renderRequired = CellSelection.ShockWaveEffect(_mapConsole);
                _nextAnimation = false;


                // Blit the sub consoles to the root console in the correct locations
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
            else if (displayMenu)
            {
                _UIConsole.Clear();
                _UIConsole.SetBackColor(0, 0, _screenWidth, _screenHeight, Colors.ComplimentLighter);
                menu.Draw(_UIConsole, GetMousePos());
                RLConsole.Blit(_UIConsole, 0, 0, _screenWidth, _screenHeight, _rootConsole, 0, 0);
                _rootConsole.Draw();
            }
            if (_gameOver && !displayMenu)
            {
                _rootConsole.Clear();
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();
                _inventoryConsole.Clear();
                _UIConsole.Clear();

                Map.Draw(_mapConsole, _statConsole, false);
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
                int offset = GetEvenlySpacedOffset(_UIHeight, 2);
                _UIConsole.Print(GetCenterOffset(_UIWidth, 9), offset, "GAME OVER", RLColor.White);
                _UIConsole.Print(GetCenterOffset(_UIWidth, 25), 2*offset + 1, "Press Enter to play again", RLColor.LightGray);

                Point mapBlitOrigin = GetMapBlitOrigin();
                RLConsole.Blit(_mapConsole, mapBlitOrigin.X, mapBlitOrigin.Y, _onConsoleMapWidth, _onConsoleMapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _onConsoleMapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);
                RLConsole.Blit(_UIConsole, 0, 0, _UIWidth, _UIHeight, _rootConsole, GetCenterOffset(_screenWidth, _UIWidth), GetCenterVerticalOffset(_screenHeight, _UIHeight));

                _rootConsole.Draw();
            }
            if (_draw)
            {
                _rootConsole.Draw();
                _draw = false;
            }
        }
        private static bool _draw = false;
        public static void GameOver()
        {
            _gameOver = true;
            _activeSelection = false;
            _renderRequired = true;
            Map.SetIsWalkable(Player.Coord, true);
        }
    }
}
