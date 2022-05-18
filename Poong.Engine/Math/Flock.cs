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

        public Flock()
        {
            Boids = new List<Boid>();
        }
        public void Update()
        {
            foreach (var boid in Boids)
            {
                boid.Speed += GetConvergeSpeed(boid, boid.ConvergePower);
                boid.Update(0);
            }
        }
        private Vector GetConvergeSpeed(Boid boid, float power)
        {
            float dVx = X + boid.FlockCenterOffset.X - boid.Center.X;
            float dVy = Y + boid.FlockCenterOffset.Y - boid.Center.Y;
            return new Vector(dVx * power, dVy * power);
        }
    }
}
