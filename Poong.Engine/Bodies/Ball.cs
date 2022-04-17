using System;

namespace Poong.Engine
{
    public class Ball : Body
    {
        private const float InitialSpeed = 0.02f;
        internal event EventHandler<BoundaryTouchingEventArgs> BoundaryTouching;
        private Boundaries LastBoundaries = Boundaries.None;
        internal Ball()
            : base(new Point(0, 0.0f), new Size(Game.pixelSize), new Vector(0))
        {
        }
        internal Ball(Point position, Size size, Vector speed)
            : base(position, size, speed)
        {

        }
        internal Ball(Ball ball) : base(ball.Center, ball.Size, ball.Speed)
        {

        }
        internal void Reset()
        {
            Center.X = 0;
            Center.Y = 0;
            Speed.X = 0;
            Speed.Y = 0;
        }
        internal void Launch(Side side)
        {
            int sign = side == Side.None ? new Random().Next(2) * 2 - 1 : side == Side.Right?1:-1;
            Speed.X = InitialSpeed * sign;
            Speed.Y = 0;
        }
    private Boundaries GetBoundariesTouching()
        {
            Boundaries boundaries = Boundaries.None;

            if (Bottom > Game.verticalHalfSize && Speed.Y > 0)
                boundaries |= Boundaries.BottomBoundary;
            else if (Top < -Game.verticalHalfSize && Speed.Y < 0)
                boundaries |= Boundaries.TopBoundary;

            if (Left < -Game.horizontalHalfSize && Speed.X < 0)
                boundaries |= Boundaries.LeftGoal;
            else if (Left < -Game.paddleFaceDistance
                && Right > -Game.paddleFaceDistance - Game.pixelSize
                && Speed.X < 0)
                boundaries |= Boundaries.LeftPaddle;
            else if (Right > Game.paddleFaceDistance
                && Left < Game.paddleFaceDistance + Game.pixelSize
                && Speed.X > 0)
                boundaries |= Boundaries.RightPaddle;
            else if (Right > Game.horizontalHalfSize && Speed.X > 0)
                boundaries |= Boundaries.RightGoal;

            return boundaries;
        }
        internal override void Update(long time)
        {
            base.Update(time);
            var currentBoundary = GetBoundariesTouching();
            //BoundaryTouching(this, new BoundaryTouchingEventArgs(Boundaries.RightPaddleFace));
            if (currentBoundary != Boundaries.None)
            {
                BoundaryTouching(this, new BoundaryTouchingEventArgs(currentBoundary, LastBoundaries));
                //if (lastBoundaries != currentBoundary)
                //    System.Diagnostics.Debug.WriteLine(currentBoundary.ToString());

            }
            LastBoundaries = currentBoundary;
        }

        internal class BoundaryTouchingEventArgs : EventArgs
        {
            public Boundaries Boundaries { get; set; }
            public Boundaries LastBoundaries { get; set; }
            public BoundaryTouchingEventArgs(Boundaries boundaries, Boundaries lastBoundaries)
            {
                Boundaries = boundaries;
                LastBoundaries = lastBoundaries;
            }
            public bool FirstCrossedBoundary(Boundaries boundary)
            {
                return Boundaries.HasFlag(boundary) && !LastBoundaries.HasFlag(boundary);
            }

        }
    }
}
