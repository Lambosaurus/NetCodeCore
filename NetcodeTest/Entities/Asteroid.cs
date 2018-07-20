using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NetCode;

namespace NetcodeTest.Entities
{
    public class Asteroid : Entity
    {
        [Synchronisable]
        public float Size { get; protected set; }

        public Asteroid() : base(new Vector2(0,0))
        {
        }

        public Asteroid( Vector2 position, Vector2 velocity, float size, float angle, float angleV ) : base(position)
        {
            Size = size;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angleV;
        }
        
        public override void Draw(SpriteBatch batch)
        {
            Drawing.DrawSquare(batch, Position, new Vector2(Size, Size), Angle, Color.White);
        }
    }
}
