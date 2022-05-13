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
            // update void speed and direction (velocity) based on rules
            if (Boids.Count > 0)
            {
                Vx = Boids.Average(boid => boid.Speed.X);
                Vy = Boids.Average(boid => boid.Speed.Y);
            }
            foreach (var boid in Boids)
            {
                (float flockXvel, float flockYvel) = Converge(boid, boid.ConvergeDistance, boid.ConvergePower);
                (float alignXvel, float alignYvel) = Align(boid, boid.AlignDistance, boid.AlignPower);
                (float avoidXvel, float avoidYvel) = Avoid(boid, boid.AvoidDistance, boid.AvoidPower);
                boid.Speed.X += (flockXvel + avoidXvel + alignXvel);
                boid.Speed.Y += (flockYvel + avoidYvel + alignYvel);
                boid.Speed.X = Math.Clamp(boid.Speed.X * 0.99f, -0.021f, 0.021f);
                boid.Speed.Y = Math.Clamp(boid.Speed.Y * 0.99f, -0.021f, 0.021f);
                boid.Update(0);
            }
        }
        private (float xVel, float yVel) Converge(Boid boid, float distance, float power)
        {
            //var neighbors = Boids.Where(x => x.GetDistance(boid) < distance);
            //float meanX = (X + neighbors.Sum(x => x.Center.X) / neighbors.Count()) / 2;
            //float meanY = (Y + neighbors.Sum(x => x.Center.Y) / neighbors.Count()) / 2;
            float meanX = X;
            float meanY = Y;
            float deltaCenterX = meanX - boid.Center.X;
            float deltaCenterY = meanY - boid.Center.Y;
            return (deltaCenterX * power, deltaCenterY * power);
        }
        private (float xVel, float yVel) Align(Boid boid, float distance, float power)
        {
            //var neighbors = Boids.Where(x => x.GetDistance(boid) < distance);
            //float meanXvel = neighbors.Sum(x => x.Speed.X) / neighbors.Count();
            //float meanYvel = neighbors.Sum(x => x.Speed.Y) / neighbors.Count();
            float meanXvel = Vx;
            float meanYvel = Vy;
            float dXvel = meanXvel - boid.Speed.X;
            float dYvel = meanYvel - boid.Speed.Y;
            return (dXvel * power, dYvel * power);
        }
        private (float xVel, float yVel) Avoid(Boid boid, float distance, float power)
        {
            //var neighbors = Boids.Where(x => x.GetDistance(boid) < distance);
            var offcenter = (float)(((Vector)(boid.Center - new Point(X, Y))).Magnitude);
            var closeness = MathF.Max(1 - offcenter/distance, 0) ;
            (float sumClosenessX, float sumClosenessY) = (
                (float)(new Random().NextDouble()-0.5)/100* closeness,
                (float)(new Random().NextDouble()-0.5)/100* closeness
                );
            //foreach (var neighbor in neighbors)
            //{
            //    float closeness = distance - boid.GetDistance(neighbor);
            //    sumClosenessX += (boid.Center.X - neighbor.Center.X) * closeness;
            //    sumClosenessY += (boid.Center.Y - neighbor.Center.Y) * closeness;
            //}
            return (sumClosenessX * power, sumClosenessY * power);
        }
    }
}
