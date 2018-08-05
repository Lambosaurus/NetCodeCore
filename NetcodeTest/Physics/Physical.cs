using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using NetcodeTest.Entities;

namespace NetcodeTest.Physics
{
    public abstract class Physical : Entity
    {
        public float Mass;
        public float Moment;
        public Hitbox Hitbox;

        public override void Update(float delta)
        {
            base.Update(delta);
            Hitbox.Update(Position, Angle);
        }

        public void Push(Vector2 force, float torque)
        {
            Velocity += force / Mass;
            AngularVelocity += torque / Moment;
            MotionUpdateRequired = true;
        }

        public void Push(Vector2 force, Vector2 eccentricity)
        {
            float torque = Util.Cross(eccentricity, force);
            this.Push(force, torque);
        }

        public bool HitCheck(Physical phys)
        {
            Intersection sect = Hitbox.Intersect(phys.Hitbox);
            if (sect == null) { return false; }

            Vector2 this_ecc = sect.position - Position;
            Vector2 phys_ecc = sect.position - phys.Position;

            // determine the velocities at the points of impact, this includes the velocity due to radial effects
            Vector2 v1 = Velocity + Util.RotatePos(this_ecc * AngularVelocity);
            Vector2 v2 = phys.Velocity + Util.RotatePos(phys_ecc * phys.AngularVelocity);

            // get the relative velocity at the impact point
            Vector2 impactVelocity = (v1 - v2);

            // modify the imact velocity in the direction of the surface normal.
            // This is based on the overlap, and functions as a spring.
            impactVelocity += (Util.CosSin(sect.surface_normal) * (0.2f + (sect.overlap * 0.05f)));

            // calculate the 'bounce' velocity.
            // The velocity which must be created between the two physicals, as a result of the collision
            Vector2 surface_aligned = Util.Rotate(impactVelocity, -sect.surface_normal);
            surface_aligned.X *= -0.9f; // bouncyness
            surface_aligned.Y *= -0.2f; // friction
            Vector2 bounce = Util.Rotate(surface_aligned, sect.surface_normal);

            float thisRelativeMass = Mass / (Mass + phys.Mass);
            float physRelativeMass = phys.Mass / (Mass + phys.Mass);

            // modify the physicals positions, to remove them from the collision.
            // this may cause problems if it pushes a ship into a second collsion. Whatevs.
            this.Position += bounce * 1.5f * physRelativeMass;
            phys.Position -= bounce * 1.5f * thisRelativeMass;

            /* Replaced by the below
            // calculate the generated foce between each ship required to produce the bounce velocity
            ector2 force = ((bounce) / ((1 / mass) + (1 / phys.mass)));
            
            Vector2 velocity_this = (force / mass) + Utility.RotatePos(eccentricity_this * Utility.Cross(eccentricity_this, force) / intertia);
            Vector2 velocity_phys = (force / phys.mass) + Utility.RotatePos(eccentricity_phys * Utility.Cross(eccentricity_phys, force) / phys.intertia)
            */

            // Calculate some constants.
            float A1 = (1.0f / this.Mass) + (1.0f / phys.Mass) + (this_ecc.Y * this_ecc.Y / this.Moment) + (phys_ecc.Y * phys_ecc.Y / phys.Moment);
            float B1 = (this_ecc.Y * this_ecc.X / this.Moment) + (phys_ecc.Y * phys_ecc.X / phys.Moment);
            float A2 = (1.0f / this.Mass) + (1.0f / phys.Mass) + (this_ecc.X * this_ecc.X / this.Moment) + (phys_ecc.X * phys_ecc.X / phys.Moment);
            float B2 = (this_ecc.X * this_ecc.Y / this.Moment) + (phys_ecc.X * phys_ecc.Y / phys.Moment);

            // Solve for the force in the X and Y directions.
            float Fx = ((B1 * bounce.Y / (A1 * A2)) + (bounce.X / A1)) / (1.0f - (B1 * B2 / (A1 * A2)));
            float Fy = ((B2 * Fx) + bounce.Y) / A2;

            Vector2 force = new Vector2(Fx, Fy);


            // apply the forces.
            this.Push(force, this_ecc);
            phys.Push(-force, phys_ecc);
            
            return true; // a collision was indeed serviced.
        }
    }
}
