using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public class Vector : Point
    {
        
        public float Angle
        {
            get
            {
                return MathF.Atan2(Y, X);
            }
        }
        public float Magnitude
        {
            get
            {
                return MathF.Sqrt(X * X + Y * Y);
            }
        }
        internal Vector(float x, float y) : base(x,y)
        {
            X = x;
            Y = y;
        }
        internal Vector(float xy):base(xy)
        {
            X = xy;
            Y = xy;
        }
        internal void FlipX()
        {
            X = -X;
        }
        internal void FlipY()
        {
            Y *= -1;
        }
        public static Vector operator *(Vector a, float b)
        {
            return new Vector(a.X*b, a.Y*b);
        }
        public static Vector operator /(Vector a, float b)
        {
            return new Vector(a.X / b, a.Y / b);
        }
    }
}
