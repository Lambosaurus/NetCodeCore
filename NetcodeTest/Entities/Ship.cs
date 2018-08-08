using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Volatile;

using NetCode;

namespace NetcodeTest.Entities
{
    public class Ship : Entity
    {
        [Synchronisable]
        public Color Color { get; protected set; }
        [Synchronisable]
        public Vector2 Size { get; protected set; }
        

        public Ship()
        {
            Color = Color.Black;
        }

        public Ship( Vector2 position, Vector2 velocity, Color color, float angle, float angleV)
        {
            Color = color;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angleV;

            Size = new Vector2(10, 20);
        }
        
        public override void Draw(SpriteBatch batch)
        {
            Drawing.DrawTriangle(batch, Position, Size, Angle, Color);
        }

        public override void GenerateBody(VoltWorld world)
        {
            float length = Size.X;
            float width = Size.Y;
            float CentroidToBack = length * (1.0f / 6.0f);

            VoltPolygon polygon = world.CreatePolygonBodySpace(
                new Vector2[]
                {
                    new Vector2(length - CentroidToBack, 0),
                    new Vector2(-CentroidToBack, -width/2),
                    new Vector2(-CentroidToBack, width/2),
                }
                );

            CollisionBody = world.CreateDynamicBody(Position, Angle, polygon);
            CollisionBody.AngularVelocity = AngularVelocity;
            CollisionBody.LinearVelocity = Velocity;
        }
    }
}
