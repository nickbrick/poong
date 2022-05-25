using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    internal class Boid : Body
    {
        public Player Player { get; }
        public float ConvergePower { get; }
        public Point FlockCenterOffset { get; }
        public float MaximumSpeed { get; }
        public float SpeedDecay { get; }
        public Boid(Player player)
            : base(player.Position, new Size(0), new Vector(RollStat(0.005f, 0.5f), RollStat(0.005f, 0.5f)))
        {
            Player = player;
            ConvergePower = RollStat(Transformation.ToRatePerTick(1.0f), 0.95f);
            FlockCenterOffset = new Point(1 - RollStat(1.0f, 0.5f), 1 - RollStat(1.0f, 0.5f));
            MaximumSpeed = RollStat(Transformation.ToRatePerTick(25.0f), 0.1f);
            SpeedDecay = MathF.Min(0.99f, RollStat(0.99f, 0.05f));
        }
        internal override void Update(long time)
        {
            Speed.X += Transformation.ToRatePerTick(RollStat(0.025f / MathF.Max(0.001f, Speed.X) / 10000, 0.1f) * Math.Sign(new Random().NextDouble() - 0.5));
            Speed.Y += Transformation.ToRatePerTick(RollStat(0.025f / MathF.Max(0.001f, Speed.Y) / 10000, 0.1f) * Math.Sign(new Random().NextDouble() - 0.5));
            Speed.X = Math.Clamp(Speed.X, -MaximumSpeed, MaximumSpeed);
            Speed.Y = Math.Clamp(Speed.Y, -MaximumSpeed, MaximumSpeed);
            Speed.X *= SpeedDecay * (1 - Math.Abs(Speed.X / MaximumSpeed));
            Speed.Y *= SpeedDecay * (1 - Math.Abs(Speed.Y / MaximumSpeed));
            base.Update(time);
        }
        private static float RollStat(float mean, float relativeError)
        {
            return mean * (1 + relativeError * ((float)new Random().NextDouble() - 0.5f));
        }
    }
}
