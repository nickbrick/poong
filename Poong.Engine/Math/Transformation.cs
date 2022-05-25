using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    /// <summary>
    /// A set of parameters that defines the way engine coordinates map to client pixel coordinates.
    /// </summary>
    public class Transformation
    {
        private float OriginX { get; set; }
        private float OriginY { get; set; }
        private float ScaleX { get; set; }
        private float ScaleY { get; set; }
        public Transformation(float originX, float originY, float scaleX, float scaleY)
        {
            OriginX = originX;
            OriginY = originY;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }


        internal Point ToScreen(Point point)
        {
            return new Point(ToScreen(point.X, Axis.X, Mode.Position), ToScreen(point.Y, Axis.Y, Mode.Position));
        }
        internal Size ToScreen(Size size)
        {
            return new Size(ToScreen(size.Width, Axis.X, Mode.Quantity), ToScreen(size.Height, Axis.Y, Mode.Quantity));
        }
        internal Vector ToScreen(Vector vector)
        {
            return new Vector(ToScreen(vector.X, Axis.X, Mode.Quantity), ToScreen(vector.Y, Axis.Y, Mode.Quantity));
        }
        internal Point ToEngine(Point point)
        {
            return new Point((point.X - OriginX) / ScaleX, (point.Y - OriginY) / ScaleY);
        }
        internal float ToScreen(float value, Axis axis, Mode mode)
        {
            return value * (axis == Axis.X ? ScaleX : ScaleY) + (mode == Mode.Position ?(axis == Axis.X ? OriginX : OriginY) : 0);
        }
        internal static float ToRatePerSecond(float ratePerTick)
        {
            return ratePerTick / Game.Config.TickMilliseconds * 1000f;
        }
        internal static float ToRatePerTick(float ratePerSecond)
        {
            return ratePerSecond * Game.Config.TickMilliseconds / 1000f;
        }
        internal static float ToSeconds(int ticks)
        {
            return ticks * Game.Config.TickMilliseconds / 1000f;
        }
        internal static int ToTicks(float seconds)
        {
            return (int)MathF.Round(seconds * 1000f / (float)Game.Config.TickMilliseconds);
        }
        internal enum Axis
        {
            X,
            Y
        }
        internal enum Mode
        {
            Position,
            Quantity
        }
    }
}
