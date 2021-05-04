using System;
using RLNET;

namespace Projet
{
    public static class Colors
    {
        public static RLColor PrimaryLightest = new RLColor(255, 208, 163);
        public static RLColor PrimaryLighter = new RLColor(254, 157, 65);
        public static RLColor Primary = new RLColor(255, 124, 0);
        public static RLColor PrimaryDarker = new RLColor(228, 111, 0);
        public static RLColor PrimaryDarkest = new RLColor(173, 84, 0);

        public static RLColor SecondaryLightest = new RLColor(241, 255, 163);
        public static RLColor SecondaryLighter = new RLColor(218, 246, 63);
        public static RLColor Secondary = new RLColor(214, 252, 0);
        public static RLColor SecondaryDarker = new RLColor(188, 221, 0);
        public static RLColor SecondaryDarkest = new RLColor(143, 168, 0);

        public static RLColor AlternateLightest = new RLColor(165, 241, 255);
        public static RLColor AlternateLighter = new RLColor(43, 138, 156);
        public static RLColor Alternate = new RLColor(0, 187, 222);
        public static RLColor AlternateDarker = new RLColor(0, 118, 140);
        public static RLColor AlternateDarkest = new RLColor(1, 90, 107);

        public static RLColor ComplimentLightest = new RLColor(249, 164, 255);
        public static RLColor ComplimentLighter = new RLColor(158, 43, 166);
        public static RLColor Compliment = new RLColor(210, 0, 225);
        public static RLColor ComplimentDarker = new RLColor(139, 0, 149);
        public static RLColor ComplimentDarkest = new RLColor(106, 0, 113);

        public static RLColor FloorBackground = RLColor.Black;
        public static RLColor Floor = new RLColor(71, 62, 45);
        public static RLColor FloorBackgroundFov = new RLColor(20, 12, 28);
        public static RLColor FloorFov = new RLColor(129, 121, 107);

        public static RLColor WallBackground = new RLColor(31, 38, 47);
        public static RLColor Wall = new RLColor(72, 77, 85);
        public static RLColor WallBackgroundFov = new RLColor(51, 56, 64);
        public static RLColor WallFov = new RLColor(93, 97, 105);

        public static RLColor DoorBackground = new RLColor(71, 56, 45);
        public static RLColor Door = new RLColor(158, 147, 138);
        public static RLColor DoorBackgroundFov = new RLColor(97, 84, 75);
        public static RLColor DoorFov = new RLColor(190, 180, 174);

        public static RLColor TextHeading = new RLColor(222, 238, 214);

        public static RLColor Player = new RLColor(222, 238, 214);

        public static RLColor Health = new RLColor(218, 0, 11);

        public static RLColor Text = new RLColor(222, 238, 214);
        public static RLColor Gold = new RLColor(218, 212, 94);

        public static RLColor KoboldColor = new RLColor(210, 125, 44);

        public static RLColor gradient1 = new RLColor(255, 30, 0);
        public static RLColor gradient2 = new RLColor(255, 233, 97);

        public static RLColor GetDarkerColor(RLColor color)
        {
            return RLColor.Blend(color, RLColor.Black, 150);
        }

        public static RLColor GetDarkerColor(RLColor color, float blendRatio)
        {
            return RLColor.Blend(color, RLColor.Black, blendRatio);
        }
    }
}