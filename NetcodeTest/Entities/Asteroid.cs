﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Volatile;

using NetCode;
using NetcodeTest.Util;

namespace NetcodeTest.Entities
{
    public class Asteroid : Physical
    {
        [Synchronisable]
        public float Size { get; protected set; }

        public const float MinimumSize = 8f;
        public const float EjectionVelocity = 3f;
        public const float EjectionRotation = 0.2f;

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
            Hitpoints = size * size / 4.0;
        }
        
        public override void Draw(SpriteBatch batch)
        {
            Drawing.DrawSquare(batch, Position, new Vector2(Size, Size), Angle, Color.LightGray);
        }

        protected override Vector2[] GetHitbox()
        {
            float half = Size / 2;
            return new Vector2[]
                {
                    new Vector2(half, half),
                    new Vector2(half, -half),
                    new Vector2(-half, -half),
                    new Vector2(-half, half),
                };
        }

        public override void OnDestroy()
        {
            float subsize = Size / 2;
            if (subsize > MinimumSize)
            {
                Vector2 ecc = Fmath.Rotate(new Vector2(Size/4, Size/4), Angle);

                for (int i = 0; i < 4; i++)
                {
                    ecc = Fmath.RotatePos(ecc);

                    Context.AddEntity(new Asteroid(
                        Position + ecc,
                        Velocity + ecc * EjectionVelocity / Size,
                        subsize,
                        Angle,
                        AngularVelocity + EjectionRotation - Fmath.RandF(2 * EjectionRotation)
                        ));
                }
            }

            base.OnDestroy();
        }
    }
}
