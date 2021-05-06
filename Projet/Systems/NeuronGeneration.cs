using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Core;
using RLNET;
using RogueSharp;

namespace Projet.Systems
{
    public class NeuronGeneration : MapGenerator
    {
        public NeuronGeneration(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize, int mapLevel) : base(width, height, maxRooms, roomMaxSize, roomMinSize, mapLevel)
        {
        }

        public override GameMap CreateMap(int seed)
        {
            base.CreateMap(seed);

            for (int i = 0; i < 100; i++)
            {
                Point laserStart = new Point(Game.Random.Next(_width), Game.Random.Next(_height));
                Point laserDir = new Point(Game.Random.Next(_width), Game.Random.Next(_height));
            }

            return _map;
        }

        private List<ICell> GetStraightLine(Cell startCell, Cell endCell)
        {
            List<ICell> line = new List<ICell>();
            int deltaX = endCell.X - startCell.X;
            int deltaY = endCell.Y - startCell.Y;
            Point step;
            Point rest;
            Point stopPoint;
            if(Math.Abs(deltaY) < Math.Abs(deltaX))
            {
                int stepY = Math.DivRem(deltaX, deltaY, out int restY);
                step = new Point(1, stepY);
                rest = new Point(0, restY);
                stopPoint = new Point(endCell.X,endCell.Y - restY);
            }
            else
            {
                int stepX = Math.DivRem(deltaY, deltaX, out int restX);
                step = new Point(stepX, 1);
                rest = new Point(restX, 0);
                stopPoint = new Point(endCell.X - restX, endCell.Y);
            }
            Point stream = new Point(startCell.X, startCell.Y);
            while(stream != stopPoint)
            {
                line.Add(_map.GetCell(stream.X, stream.Y));

            }
            return line;
        }
    }
}
