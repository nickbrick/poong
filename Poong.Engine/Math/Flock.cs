using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poong.Engine
{
    internal class Flock
    {
        public List<Boid> Boids { get; set; }
        public float X = 0;
        public float Y = 0;
        public float Vx = 0;
        public float Vy = 0;

        public Flock()
        {
            Boids = new List<Boid>();
        }
        public void Update()
        {
            if (Boids.Count > 0)
            {
                Vx = Boids.Average(boid => boid.Speed.X);
                Vy = Boids.Average(boid => boid.Speed.Y);
            }
            foreach (var boid in Boids)
            {
                (float convergeVx, float convergeVy) = Converge(boid, boid.ConvergePower);
                (float alignVx, float alignVy) = Align(boid, boid.AlignPower);
                boid.Speed.X += (convergeVx + alignVx);
                boid.Speed.Y += (convergeVy + alignVy);
                boid.Speed.X = Math.Clamp(boid.Speed.X * 0.99f, -0.021f, 0.021f);
                boid.Speed.Y = Math.Clamp(boid.Speed.Y * 0.99f, -0.021f, 0.021f);
                boid.Update(0);
            }
        }
        private (float Vx, float Vy) Converge(Boid boid, float power)
        {
            float dVx = X + boid.FlockCenterOffset.X - boid.Center.X;
            float dVy = Y + boid.FlockCenterOffset.Y - boid.Center.Y;
            return (dVx * power, dVy * power);
        }
        private (float Vx, float Vy) Align(Boid boid, float power)
        {
            float dVx = Vx - boid.Speed.X;
            float dVy = Vy - boid.Speed.Y;
            return (dVx * power, dVy * power);
        }
    }
}
