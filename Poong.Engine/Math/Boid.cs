using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    internal class Boid : Body
    {
        public Player Player { get; set; }
        public float ConvergePower { get; set; }
        public float AlignPower { get; set; }
        public float AvoidPower { get; set; }
        public Point FlockCenterOffset { get; }
        public Boid(Player player)
            : base(player.Position, new Size(0), new Vector(RollStat(0.01f, 0.5f), RollStat(0.01f, 0.5f)))
        {
            Player = player;
            ConvergePower = RollStat(0.01f, 0.951f);
            AlignPower = RollStat(0.00f, 0.951f);
            AvoidPower = RollStat(10.1021f, 0.951f);
            FlockCenterOffset = new Point(1-RollStat(1.0f, 0.5f), 1-RollStat(1.0f, 0.5f));
        }

        internal float GetDistance(Boid neighbor)
        {
            return MathF.Sqrt(MathF.Pow(Center.X - neighbor.Center.X, 2) + MathF.Pow(Center.Y - neighbor.Center.Y, 2));
        }
        internal override void Update(long time)
        {
            Speed.X += RollStat(0.0051f / MathF.Max(0.001f, Speed.X) / 10000, 0.1f);
            Speed.Y += RollStat(0.0051f / MathF.Max(0.001f, Speed.Y) / 10000, 0.1f);
            Speed.X = Math.Clamp(Speed.X, -0.01f, 0.01f);
            Speed.Y = Math.Clamp(Speed.Y, -0.51f, 0.51f);
            base.Update(time);
        }
        private static float RollStat(float mean, float relativeError)
        {
            return mean * (1 + relativeError * ((float)new Random().NextDouble() - 0.5f));
        }
    }
}
