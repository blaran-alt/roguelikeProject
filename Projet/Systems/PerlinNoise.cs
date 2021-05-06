using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using System.Net;
using SimplexNoise;

namespace Projet.Systems
{
    public static class PerlinNoise
    {

        public static float[,] GeneratePerlinNoiseMap(int width, int height, int seed)
        {
            Noise.Seed = seed;
            float[,] noiseMap = Noise.Calc2D(width, height, .2f);
            return noiseMap;
        }
    }
}
