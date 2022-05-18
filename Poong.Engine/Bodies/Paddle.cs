using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poong.Engine
{
    public class Paddle : Body
    {
        public Side Side { get; set; }
        internal List<Player> Players { get; set; }
        public float Length
        {
            get { return base.Size.Height; }
            internal set { base.Size.Height = value; }
        }
        internal Paddle(Side side, float length)
            : base(
                  new Vector(side == Side.Left ? -Game.Config.HorizontalHalfSize + Game.Config.PaddleFaceDistance - Game.Config.PixelSize / 2.0f 
                                                : Game.Config.HorizontalHalfSize - Game.Config.PaddleFaceDistance + Game.Config.PixelSize / 2.0f, 0),
                  new Size(Game.Config.PixelSize, length),
                  new Vector(0)
                  )
        {
            Players = new List<Player>(Game.Config.MaxPlayersPerPaddle);
        }
        internal float GetNormalizedDistanceFromCenter(Ball ball)
        {
            if (ball.Center.Y > Center.Y)
                return (ball.Top - Center.Y) / HalfHeight;
            return (ball.Bottom - Center.Y) / HalfHeight;
        }
        internal bool IsBallTouching(Ball ball)
        {
            var distance = GetNormalizedDistanceFromCenter(ball);
            return (-1.0f < distance && distance < 1.0f);
        }
        internal override void Update(long time)
        {
            if ((Players?.Count ?? 0) > 0)
                Speed = new Vector(0, Players.Select(player => player.Position.Y).Average()-Center.Y);
            base.Update(time);
            //Center.Y = Input;
        }
    }

}
