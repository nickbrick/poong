using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class Body
    {
        public Point Center { get; set; }
        public Point Corner
        {
            get { return new Point(Center.X - HalfWidth, Center.Y - HalfHeight); }
        }
        public Size Size { get; set; }
        public Vector Speed { get; set; }
        public float Top { get { return Center.Y - HalfHeight; } }
        public float Bottom { get { return Center.Y + HalfHeight; } }
        public float Left { get { return Center.X - HalfWidth; } }
        public float Right { get { return Center.X + HalfWidth; } }
        internal float HalfWidth { get { return Size.Width / 2.0f; } }
        internal float HalfHeight { get { return Size.Height / 2.0f; } }

        internal Body(Point center, Size size, Vector speed)
        {
            Center = center;
            Size = size;
            Speed = speed;
        }
        internal virtual void Update(long time)
        {
            Center.X += Speed.X;
            Center.Y += Speed.Y;
        }
    }
}
