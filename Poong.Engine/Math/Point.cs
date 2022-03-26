namespace Poong.Engine
{
    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
        internal Point(float x, float y)
        {
            X = x;
            Y = y;
        }
        internal Point(float xy)
        {
            X = xy;
            Y = xy;
        }
        public static Point operator -(Point a, Point b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }
    }
}