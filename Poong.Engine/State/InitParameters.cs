using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class InitParameters
    {
        public float VerticalHalfSize = Game.verticalHalfSize;
        public float PaddleFaceDistance = Game.paddleFaceDistance;
        public float HorizontalHalfSize = Game.horizontalHalfSize;
        public float PaddleWidth { get; set; }
        public float PaddleHeight { get; set; }
        public float BallWidth { get; set; }
        public float BallHeight { get; set; }
    }
}
