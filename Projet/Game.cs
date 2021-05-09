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
        private static readonly int _screenHeight = GetProportionnalHorizontalSize(55);
        public static RLRootConsole _rootConsole;


        public static int GetProportionnalHorizontalSize(int value)
        {
            return GetProportionnalSizeInContainer(value, _screenWidth);
        }
        public static int GetProportionnalVerticalSize(int value)
        {
            return GetProportionnalSizeInContainer(value, _screenHeight);
        }
        public static Point GetProportionnalSize(int x, int y)
        {
            return new Point(GetProportionnalHorizontalSize(x),GetProportionnalVerticalSize(y));
        }
        public static int GetProportionnalSizeInContainer(int value, int containerLength)
        {
            return Math.Max((int)Math.Round(value * containerLength/100f), 1);
        }
        public static Point GetProportionnalSizeInContainer(int x, int y, Point containerSize)
        {
            return new Point(GetProportionnalSizeInContainer(x, containerSize.X), GetProportionnalSizeInContainer(y, containerSize.Y));
        }


        // The map console takes up most of the screen and is where the map will be drawn
        private static readonly int _onConsoleMapWidth = GetProportionnalHorizontalSize(80);
        private static readonly int _onConsoleMapHeight = GetProportionnalVerticalSize(70);
        private static readonly int _mapWidth = 120;
        private static readonly int _mapHeight = 60;
        public static RLConsole _mapConsole;

        // The stat console is to the right of the map and display player and monster stats
        private static readonly int _statWidth = _screenWidth - _onConsoleMapWidth;
        private static readonly int _statHeight = _screenHeight;
        private static RLConsole _statConsole;

        // Above the map is the inventory console which shows the players equipment, abilities, and items
        private static readonly int _inventoryWidth = GetProportionnalHorizontalSize(80);
        private static readonly int _inventoryHeight = GetProportionnalVerticalSize(20);
        private static RLConsole _inventoryConsole;

        // Below the map console is the message console which displays attack rolls and other information
        private static readonly int _messageWidth = GetProportionnalHorizontalSize(80);
        private static readonly int _messageHeight = _screenHeight - _onConsoleMapHeight - _inventoryHeight;
        private static RLConsole _messageConsole;

        private static int _UIWidth = _screenWidth;
        private static int _UIHeight = _screenHeight; 
        private static RLConsole _UIConsole;

        public static Player Player { get;  set; }
        public static GameMap Map { get; private set; }
        public static CommandSystem CommandSystem { get; private set; }
        public static MessageLog MessageLog { get; private set; }
        public static SchedulingSystem SchedulingSystem { get; private set; }
        public static Inventory Inventory { get; set; }

        public static IRandom Random { get; private set; }
        private static int _seed;

        private static Save _save;

        private static bool _renderRequired = true;
        private static int _mapLevel;
        public static int Level { get { return _mapLevel; } }
        private static bool _gameOver;
        private static Point lastMousePos;

        private static Menu menu;

        private static bool playWithSeed;

        public static void Main()
        {
            _gameOver = true;

            menu = new MainMenu(_screenWidth, _screenHeight);
            numberOnly = true;

            string fontFileName = "ExtendTestBis.png";

            // Donne le fichier de caractere a utiliser ainsi que leur taille (16 x 16 pixels)
            _rootConsole = new RLRootConsole(fontFileName, _screenWidth, _screenHeight,
              16, 16, 1f, "Game Name");
            _rootConsole.SetWindowState(RLWindowState.Fullscreen);

            // Initialise la console principale sur laquelle viendront se coller toutes les autres
            _mapConsole = new RLConsole(_mapWidth, _mapHeight); // Console  qui affiche la carte
            _messageConsole = new RLConsole(_messageWidth, _messageHeight); // Console  qui affiche les messages
            _statConsole = new RLConsole(_statWidth, _statHeight);// Console  qui affiche les stats du joueurs et les monstres aux alentours
            _inventoryConsole = new RLConsole(_inventoryWidth, _inventoryHeight);// Console qui affiche l'inventaire
            _UIConsole = new RLConsole(_screenWidth, _screenHeight);// Console qui affiche les menus

            // Set up a handler for RLNET's Update event
            _rootConsole.Update += OnRootConsoleUpdate;
            // Set up a handler for RLNET's Render event
            _rootConsole.Render += OnRootConsoleRender;

            // Set background color and text for each console
            _rootConsole.SetBackColor(0, _inventoryHeight, _onConsoleMapWidth, _onConsoleMapHeight, Colors.Primary);
            _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Colors.Alternate);
            _statConsole.Print(1, 1, "Stats", RLColor.White);

            _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
            _inventoryConsole.Print(1, 1, "Inventaire", RLColor.White);

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
            Point pos = GetMousePos();
            if(Map is InvertedMap)
            {
                pos = new Point(_onConsoleMapWidth - pos.X, _onConsoleMapHeight - pos.Y + 2 * _inventoryHeight);
            }
            return GetMapToConsoleCoord(pos);
        }
        public static Point GetMousePos()
        {
            return new Point(_rootConsole.Mouse.X, _rootConsole.Mouse.Y);
        }

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

            MapGenerator mapGenerator = ChoseMapGenerator();
            Map = mapGenerator.CreateMap(_seed);

            MessageLog = new MessageLog(_messageHeight - 2);
            MessageLog.Add($"{Player.Name} arrive au niveau {_mapLevel}");
            MessageLog.Add($"Niveau creer avec la seed '{_seed}'");

            Map.UpdatePlayerFieldOfView();
            Player.Reset();

            _gameOver = false;
            MessageLog.Add(Story.levelBeginnigTexts[_mapLevel - 1]);
        }

        public static void SaveData(object sender, EventArgs args)
        {
            (sender as TextArea).IsDisabled = true;
            MenuEventArgs menuArgs = args as MenuEventArgs;
            _save.SaveInFile(menuArgs.Value.ToString());
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

            bool leftClick = _rootConsole.Mouse.GetLeftClick();

            if (leftClick && _gameOver)
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
                    string input = keyPress.Key.ToString();
                    if (inputText.Value == inputText.DefaultValue)
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
                    else if(input.Length == 7 && input.Substring(0,6) == "Number")
                    {
                        char inputChar = input[6];
                        inputText.Value += inputChar;
                    }
                    else if(!numberOnly && input.Length == 1 && IsLetter(input[0]))
                    {
                        inputText.Value += input[0];
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
                        _rootConsole.Close();
                    }
                }
            }

            if (!_gameOver)
            {
                if(keyPress != null)
                {
                    if (keyPress.Key == RLKey.Q)
                    {
                        if (Inventory.UsePotion())
                        {
                            _renderRequired = true;
                        }
                    }
                    else if(keyPress.Key == RLKey.C)
                    {
                        if (Inventory.PreviousSelection())
                        {
                            _renderRequired = true;
                        }
                    }
                    else if (keyPress.Key == RLKey.V)
                    {
                        if (Inventory.NextSelection())
                        {
                            _renderRequired = true;
                        }
                    }
                    else if (keyPress.Key == RLKey.E)
                    {
                        Map.UpdateExitState();
                        Terminal terminal = Map.GetTerminalAt(Player.X, Player.Y);
                        if (terminal != null)
                        {
                            terminal.isActive = !terminal.isActive;
                            Map.UpdateExitState();
                            _renderRequired = true;
                        }
                        foreach (ICell cell in Map.GetBorderCellsInCircle(Player.X, Player.Y, 1))
                        {
                            Box box = Map.GetBoxAt(cell.X, cell.Y);
                            if(box != null)
                            {
                                Point temp = Player.Coord;
                                Player.Coord = box.Coord;
                                box.Coord = temp;
                                _renderRequired = true;
                                break;
                            }
                        }
                        foreach(ICell cell in Map.GetBorderCellsInCircle(Player.X, Player.Y, 2))
                        {
                            Box box = Map.GetBoxAt(cell.X, cell.Y);
                            if (box != null)
                            {
                                Cell newPos = Map.GetCLosestCell(box.Coord);
                                if(newPos != null)
                                {
                                    Map.SetIsWalkable(box.X, box.Y, true);
                                    box.X = newPos.X;
                                    box.Y = newPos.Y;
                                    Map.SetIsWalkable(box.X, box.Y, false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Point mousePos = new Point(_rootConsole.Mouse.X, _rootConsole.Mouse.Y);
                    if(_gameOver)
                    if (mousePos != lastMousePos)
                    {
                        _renderRequired = true;
                    }
                    lastMousePos = mousePos;
                }
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
                        else if (keyPress.Key == RLKey.R)
                        {
                            if (Map.CanMoveDownToNextLevel())
                            {
                                if(++_mapLevel == 3)
                                {
                                    _rootConsole.LoadBitmap("ExtendTestBisInverted.png", 16, 16);
                                    Inventory.UseKeys();
                                }
                                Thread thread = new Thread(LoadNextLevel);
                                thread.Start();

                                DisplayAsync();

                                thread.Join();

                                MessageLog.Add($"{Player.Name} arrive au niveau {_mapLevel}");
                                MessageLog.Add($"Niveau creer avec la seed '{_seed}'");

                                didPlayerAct = true;
                            }
                        }
                        if (selectionChanged)
                        {
                            _renderRequired = true;
                        }
                    }
                    else if (leftClick)
                    {
                        Point mousePos = GetMousePosOnMap();
                        Monster monster = Map.GetMonsterAt(mousePos.X, mousePos.Y);
                        if (monster != null && Map.IsInFov(monster.X, monster.Y))
                        {
                            CommandSystem.Attack(Player, monster);
                            didPlayerAct = true;
                        }
                    }
                    if (didPlayerAct)
                    {
                        _renderRequired = true;
                        CommandSystem.EndPlayerTurn();
                    }
                }
                else
                {
                    CommandSystem.ActivateMonsters();
                    _renderRequired = true;
                }
            }
        }

        private static bool IsLetter(char input)
        {
            int asciiIndex = (int)input;
            return ((asciiIndex > 96 && asciiIndex < 123) || (asciiIndex > 64 && asciiIndex < 91));
        }

        private static bool numberOnly;

        public static void ReloadBitmap()
        {
            _rootConsole.LoadBitmap("ExtendTestBis.png", 16, 16);
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
            if(_mapLevel == 4)
            {
                GameOver();
                return;
            }

            if (playWithSeed && _mapLevel <= _save.Seeds.Count)
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
            MapGenerator mapGenerator = ChoseMapGenerator();
            Map = mapGenerator.CreateMap(_seed);
            MessageLog = new MessageLog();
            CommandSystem = new CommandSystem();

            MessageLog.Add(Story.levelBeginnigTexts[_mapLevel - 1]);
        }

        private static MapGenerator ChoseMapGenerator()
        {
            switch (_mapLevel)
            {
                case 1:
                    return new CortexMapGenerator(_mapWidth, _mapHeight);
                case 2:
                    return new NeuronMapGenerator(_mapWidth, _mapHeight, 30, 15, 5);
                case 3:
                    return new InvertedMapGenerator(_mapWidth, _mapHeight);
                default:
                    return null;
            }
        }

        private static void DisplayAsync()
        {
            string text = Story.TransitionTexts[_mapLevel - 1];
            _UIConsole.Resize(_screenWidth, _screenHeight);
            _UIConsole.Clear();
            _UIConsole.SetChar(1, 5, '_');
            int i = 1;
            int j = 5;
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
                    _UIConsole.SetChar(i , j, ' ');
                    j += 2;
                    i = 1;
                }
            }
            Thread.Sleep(1000);
        }

        public static void Quit(object sender, EventArgs args)
        {
            _rootConsole.Close();
        }

        public static void Start(object sender, EventArgs args)
        {
            playWithSeed = false;
            _gameOver = false;
            _mapLevel = 1;
            InitializeGame();
        }

        public static void OpenSeedMenu(object sender, EventArgs args)
        {
            menu = new ChoseSeedMenu(_screenWidth, _screenHeight);
        }

        public static void OpenSaveMenu(Save save)
        {
            _save = save;
            menu = new ChoseLevel(_screenWidth, _screenHeight, _save);
        }

        public static void TakeInput(object sender, EventArgs args)
        {
            takeTextInput = true;
            inputText = (TextArea)sender;
        }

        public static void StartWithSeed(object sender, MenuEventArgs args)
        {
            _seed = int.Parse(args.Value.ToString());
            if(_save == null)
            {
                playWithSeed = false;
                _save = new Save();
                _save.Seeds.Add(_seed);
                _mapLevel = 1;
            }
            else
            {
                playWithSeed = true;
                if (_save.Seeds.Count > 1)
                {
                    _mapLevel = _save.Seeds.FindIndex(s => s == _seed) + 1;
                    // Si jamais on ne trouve pas la seed et donc le niveau correspondant
                    if (_mapLevel == -1)
                    {
                        _mapLevel = 1;
                    }
                }
                else
                {
                    _mapLevel = 1;
                }
            }
            InitializeGame();
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
            if (_renderRequired && !_gameOver)
            {
                _rootConsole.Clear();
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();
                _inventoryConsole.Clear();

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
                _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Colors.Compliment);
                _inventoryConsole.Print(1, 1, "Inventaire", RLColor.White);

                Map.Draw(_mapConsole, _statConsole, _nextAnimation);
                Player.Draw(_mapConsole, Map, _nextAnimation);
                MessageLog.Draw(_messageConsole);
                Player.DrawStats(_statConsole);
                Inventory.DrawWithEffect(_inventoryConsole, _mapConsole);

                ICell cell = Map.GetCell(Player.X, Player.Y);

                Point mousePos = GetMousePosOnMap();
                if (Map.IsInFov(mousePos.X, mousePos.Y) && Map.GetMonsterAt(mousePos.X, mousePos.Y) != null)
                {
                    CellSelection.DrawPath(Player.Coord, mousePos, _mapConsole);
                    _mapConsole.SetBackColor(mousePos.X, mousePos.Y, Colors.AlternateDarker);
                }

                _renderRequired = CellSelection.ShockWaveEffect(_mapConsole);
                _nextAnimation = false;

                if(Map is InvertedMap invertedMap){
                    _mapConsole = invertedMap.InvertMap(_mapConsole);
                    mapBlitOrigin = new Point(_mapWidth - mapBlitOrigin.X - _onConsoleMapWidth, _mapHeight - mapBlitOrigin.Y - _onConsoleMapHeight);
                }

                // Blit the sub consoles to the root console in the correct locations
                RLConsole.Blit(_mapConsole, mapBlitOrigin.X, mapBlitOrigin.Y, _onConsoleMapWidth, _onConsoleMapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _onConsoleMapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);

                // Tell RLNET to draw the console that we set
                _rootConsole.Draw();
            }
            else if (_gameOver)
            {
                _UIConsole.Clear();
                _UIConsole.SetBackColor(0, 0, _screenWidth, _screenHeight, Colors.ComplimentLighter);
                menu.Draw(_UIConsole, GetMousePos());
                RLConsole.Blit(_UIConsole, 0, 0, _UIWidth, _UIHeight, _rootConsole, 0, 0);
                _rootConsole.Draw();
            }
        }


        public static void GameOver()
        {
            _gameOver = true;
            _renderRequired = true;
            numberOnly = false;
            Map.SetIsWalkable(Player.Coord, true);
            menu = new GameOverMenu(_UIWidth, _UIHeight);
        }
    }
}
