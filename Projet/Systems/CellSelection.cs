using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using RogueSharp;
using Projet.Core;

namespace Projet.Systems
{
    public enum SelectionType
    {
        Radius = 0,
        Area = 1,
        RadiusBorder = 2,
        AreaBorder = 3,
        Column = 4,
        Row = 5,
        ColumnAndRow = 6,
        Cross = 7
    }

    public static class CellSelection
    {
        private static IEnumerable<ICell> SelectCellsAroundPoint(Point point, SelectionType selection, int selectionSize, bool highlightWalls)
        {
            int x = point.X;
            int y = point.Y;
            if (!Game.Map.IsInMap(point))
            {
                return new List<ICell>();
            }
            IEnumerable<ICell> selectedCells;
            switch (selection)
            {
                case SelectionType.Radius:
                    {
                        selectedCells = Game.Map.GetCellsInCircle(x, y, selectionSize);
                        break;
                    }
                case SelectionType.Area:
                    {
                        selectedCells = Game.Map.GetCellsInSquare(x, y, selectionSize);
                        break;
                    }
                case SelectionType.RadiusBorder:
                    {
                        selectedCells = Game.Map.GetBorderCellsInCircle(x, y, selectionSize);
                        break;
                    }
                case SelectionType.AreaBorder:
                    {
                        selectedCells = Game.Map.GetBorderCellsInSquare(x, y, selectionSize);
                        break;
                    }
                case SelectionType.Row:
                    {
                        selectedCells = Game.Map.GetCellsInRows(y);
                        break;
                    }
                case SelectionType.Column:
                    {
                        selectedCells = Game.Map.GetCellsInColumns(x);
                        break;
                    }
                case SelectionType.ColumnAndRow:
                    {
                        List<ICell> rowCells = Game.Map.GetCellsInRows(y).ToList();
                        rowCells.AddRange(Game.Map.GetCellsInColumns(x));
                        selectedCells = rowCells;
                        break;
                    }
                case SelectionType.Cross:
                    {
                        if (x < 1 || x >= Game.Map.Width - 1 || y < 1 || y >= Game.Map.Height - 1)
                        {
                            return new List<ICell>();
                        }
                        List<ICell> rowCells =Game. Map.GetCellsInRows(y + 1, y - 1).ToList();
                        rowCells.AddRange(Game.Map.GetCellsInColumns(x + 1, x - 1));
                        selectedCells = rowCells;
                        break;
                    }
                default:
                    {
                        selectedCells = Game.Map.GetCellsInCircle(x, y, selectionSize);
                        break;
                    }
            }
            if (highlightWalls)
            {
                return selectedCells;
            }
            return FilterWalls(selectedCells);
        }

        private static IEnumerable<ICell> FilterWalls(IEnumerable<ICell> cells)
        {
            return cells.Where(c => c.IsWalkable || Game.Map.GetMonsterAt(c.X,c.Y) != null);
        }

        public static void DrawCellsAroudPoint(Point point, SelectionType selection, int selectionSize, bool highlightWalls, RLConsole console)
        {
            foreach (ICell cell in SelectCellsAroundPoint(point, selection, selectionSize, highlightWalls))
            {
                if (Game.Map.IsInMap(cell) && (highlightWalls || cell.IsExplored))
                {
                    console.SetBackColor(cell.X, cell.Y, RLColor.Yellow);
                }
            }
        }
        public static void DrawCellsAroudPoint(Point point, SelectionType selection, int selectionSize, bool highlightWalls, RLConsole console, RLColor color)
        {
            foreach (ICell cell in SelectCellsAroundPoint(point, selection, selectionSize, highlightWalls))
            {
                if (Game.Map.IsInMap(cell) && (highlightWalls || cell.IsExplored))
                {
                    console.SetBackColor(cell.X, cell.Y, color);
                }
            }
        }

        public static void HighlightMonstersAround(Point center, int radius, RLConsole console)
        {
            foreach(ICell cell in SelectCellsAroundPoint(center, SelectionType.Radius, radius, false))
            {
                if(Game.Map.GetMonsterAt(cell.X, cell.Y) != null && Game.Map.IsInFov(cell.X, cell.Y))
                {
                    console.SetBackColor(cell.X, cell.Y, Colors.AlternateDarkest);
                }
            }
        }

        // Draws the walls around a width by height room, counting the walls
        public static void CreateRoomWalls(int topLeftX, int topLeftY, int width, int height, int verticalChar, int horizontalChar, int topLeftCornerChar, int topRightCornerChar, int bottomLeftCornerChar, int bottomRightCornerChar, RLConsole console)
        {
            // Top left corner
            console.SetChar(topLeftX, topLeftY, topLeftCornerChar);
            // Top right corner
            console.SetChar(topLeftX + width - 1, topLeftY, topRightCornerChar);
            // Top line
            console.SetChar(topLeftX + 1, topLeftY, width - 2, 1, horizontalChar);
            // Bottom line
            console.SetChar(topLeftX + 1, topLeftY + height - 1, width - 2, 1, horizontalChar);
            // Left line
            console.SetChar(topLeftX, topLeftY + 1, 1, height - 2, verticalChar);
            // Right line
            console.SetChar(topLeftX + width - 1 , topLeftY + 1, 1, height - 2, verticalChar);
            // Bottom left corner
            console.SetChar(topLeftX, topLeftY + height - 1, bottomLeftCornerChar);
            // Bottom right corner
            console.SetChar(topLeftX + width - 1, topLeftY + height - 1, bottomRightCornerChar);
        }
        public static void CreateRoomWalls(Point topLeftCoord, Point size, int verticalChar, int horizontalChar, int topLeftCornerChar, int topRightCornerChar, int bottomLeftCornerChar, int bottomRightCornerChar, RLConsole console)
        {
            CreateRoomWalls(topLeftCoord.X, topLeftCoord.Y, size.X, size.Y, verticalChar, horizontalChar, topLeftCornerChar, topRightCornerChar, bottomLeftCornerChar, bottomRightCornerChar, console);
        }

        private static bool shockwave = false;
        private static int shockWaveIndex;
        private static int shockWaveRadius;
        private static Point shockWaveCenter;
        public static void StartShochWaveEffect(Point center, int radius)
        {
            shockwave = true;
            shockWaveRadius = radius;
            shockWaveIndex = 0;
            shockWaveCenter = center;

        }
        public static bool ShockWaveEffect( RLConsole console)
        {
            if (shockwave)
            {
                if(shockWaveIndex <= shockWaveRadius)
                {
                    DrawCellsAroudPoint(shockWaveCenter, SelectionType.RadiusBorder, shockWaveIndex, false, console, Colors.FloorBackgroundFov);
                    DrawCellsAroudPoint(shockWaveCenter, SelectionType.RadiusBorder, shockWaveIndex, false, console);
                }
                if (shockWaveIndex - 3 > 0)
                {
                    DrawCellsAroudPoint(shockWaveCenter, SelectionType.RadiusBorder, shockWaveIndex - 3, false, console, Colors.FloorBackgroundFov);
                }
                if(++shockWaveIndex > shockWaveRadius + 3)
                {
                    shockwave = false;
                }
            }
            return shockwave;
        }
    }
}
