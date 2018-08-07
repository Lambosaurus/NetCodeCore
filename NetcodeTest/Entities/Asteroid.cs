using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Volatile;

using NetCode;

namespace NetcodeTest.Entities
{
    public class Asteroid : Entity
    {
        [Synchronisable]
        public float Size { get; protected set; }

        public Asteroid()
        {
        }

        public Asteroid( Vector2 position, Vector2 velocity, float size, float angle, float angleV)
        {
            Position = position;
            Size = size;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angleV;
        }
        
        public override void Draw(SpriteBatch batch)
        {
            Drawing.DrawSquare(batch, Position, new Vector2(Size, Size), Angle, Color.White);
        }

        public override void GenerateBody(VoltWorld world)
        {
            VoltPolygon polygon = world.CreatePolygonBodySpace(
                new Vector2[]
                {
                    new Vector2(Size, Size),
                    new Vector2(Size, -Size),
                    new Vector2(-Size, -Size),
                    new Vector2(-Size, Size),
                }
                );

            CollisionBody = world.CreateDynamicBody(Position, Angle, polygon);
            CollisionBody.AngularVelocity = AngularVelocity;
            CollisionBody.LinearVelocity = Velocity;
        }
    }
}
