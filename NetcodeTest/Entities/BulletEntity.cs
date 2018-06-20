using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using NetCode;

namespace NetcodeTest.Entities
{
    public class BulletEntity : Entity
    {
        int age = 0;

        public BulletEntity()
        {
        }
        
        public BulletEntity(Vector2 starting_location, double angle)
        {
            position = starting_location;
            velocity = Util.Angle(angle) * 2;
        }

        public override void Draw(SpriteBatch batch, bool local)
        {
            Color c = Color.Yellow;
            if (local) { c *= 0.5f; }
            batch.Draw(sq_texture, position, null, c, 0f, sq_center, 0.5f, SpriteEffects.None, 0);
        }

        public override void Update()
        {
            age++;
            if (age > 150)
            {
                expired = true;
            }
            base.Update();
        }
    }
}
