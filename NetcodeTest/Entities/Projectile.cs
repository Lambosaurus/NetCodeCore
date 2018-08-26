using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NetCode;
using NetcodeTest.Util;

namespace NetcodeTest.Entities
{
    public class Projectile : Entity
    {
        [Synchronisable(SyncFlags.Reference)]
        public Ship Creator;

        const float Speed = 500f;
        double Duration = 3.0f;
        double Damage = 20;

        float Recoil = 250;
        float Force = 1000f;

        public Projectile()
        {
        }
        
        public Projectile(Ship creator)
        {
            Position = creator.Position;
            Angle = creator.Angle;
            Velocity = creator.Velocity + Fmath.CosSin(Angle, Speed);
            AngularVelocity = 0f;

            Position += Fmath.CosSin(Angle) * creator.Size.X / 2;

            Creator = creator;

            Creator.Push(-Fmath.CosSin(Angle, Recoil));
        }

        public override void Update(float delta)
        {
            Duration -= delta;
            if (Duration <= 0) { IsDestroyed = true; }
            base.Update(delta);
        }

        public override void Draw(SpriteBatch batch)
        {
            Color color = (Creator == null) ? Color.White : Color.Lerp(Creator.Color, Color.White, 0.5f);
            Drawing.DrawBullet(batch, Position, new Vector2(20, 4), Angle, color);
        }

        public virtual void OnCollide( Physical phys )
        {
            phys.Hitpoints -= Damage;
            IsDestroyed = true;

            phys.Push(this.Velocity * this.Force / Speed, Position);
        }
    }
}
