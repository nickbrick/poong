using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poong.Engine
{
    internal class Flock
    {
        public List<Boid> Boids { get; set; }
        public float X=0;
        public float Y=0;
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
                (float flockXvel, float flockYvel) = Converge(boid, boid.ConvergeDistance, boid.ConvergePower);
                (float alignXvel, float alignYvel) = Align(boid, boid.AlignDistance, boid.AlignPower);
                boid.Speed.X += (flockXvel + alignXvel);
                boid.Speed.Y += (flockYvel + alignYvel);
                boid.Speed.X = Math.Clamp(boid.Speed.X * 0.99f, -0.021f, 0.021f);
                boid.Speed.Y = Math.Clamp(boid.Speed.Y * 0.99f, -0.021f, 0.021f);
                boid.Update(0);
            }
        }
        private (float xVel, float yVel) Converge(Boid boid, float distance, float power)
        {
            float deltaCenterX = X + boid.FlockCenterOffset.X - boid.Center.X;
            float deltaCenterY = Y + boid.FlockCenterOffset.Y - boid.Center.Y;
            return (deltaCenterX * power, deltaCenterY * power);
        }
        private (float xVel, float yVel) Align(Boid boid, float distance, float power)
        {
            float dXvel = Vx - boid.Speed.X;
            float dYvel = Vy - boid.Speed.Y;
            return (dXvel * power, dYvel * power);
        }
    }
}
