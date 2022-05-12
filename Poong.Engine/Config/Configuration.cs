using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class Configuration
    {
        public float TickMilliseconds  = 20.0f;
        public int TicksPerUpdate = 5;
        public float PixelSize = 0.04f;
        public float VerticalHalfSize = 1.0f;
        public float PaddleFaceDistance = 0.89f;
        public float HorizontalHalfSize = 1.3f;
        public float PaddleInitialLength => PixelSize * 10.0f;
        public float PaddleDecay = 0.04f;
        public float PowerPaddleLengthThrehsold = 0;
        public float PowerPaddleVerticalDeflectSpeed = 0.2f;
        public int MaxPlayersPerPaddle = 512;
        public bool KickAfk = false;
        public bool UseBoids = true;
        public bool RenderAllNames = false;
        public int BoidsPerPaddle = 4;

        public float BallInitialSpeed = 0.02f;
    }
}