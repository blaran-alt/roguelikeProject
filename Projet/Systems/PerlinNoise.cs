using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using System.Net;
using RandomAccessPerlinNoise;
using SimplexNoise;

namespace Projet.Systems
{
    public static class PerlinNoise
    {

        public static float[,] GeneratePerlinNoiseMap(int width, int height, int seed)
        {
            Noise.Seed = seed;
            float[,] noiseMap = Noise.Calc2D(width, height, .2f);
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        if (noiseMap[x, y] < 90)
            //        {
            //            noiseMap[x, y] = 0;
            //        }
            //        else
            //        {
            //            noiseMap[x, y] = 255;
            //        }
            //    }
            //}
            return noiseMap;
        }
    }
}
