using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using NetCode;

namespace NetcodeTest.Entities
{
    public class PlayerEntity : Entity
    {
        [Synchronisable]
        public ColorNo color { get; set; }

        public enum ColorNo { Red, Green, Blue };
        static Color[] colortable = new Color[] { new Color(255, 0, 0), new Color(0, 255, 0), new Color(0, 0, 255) };

        public PlayerEntity()
        {
            color = ColorNo.Red;
        }

        public PlayerEntity(Vector2 starting_location, ColorNo arg_color)
        {
            position = starting_location;
            color = arg_color;
        }

        public override void Draw(SpriteBatch batch, bool local)
        {
            Color c = colortable[(int)color];
            if (local) { c *= 0.5f; }
            batch.Draw(sq_texture, position, null, c, 0f, sq_center, 1f, SpriteEffects.None, 0);
        }
    }
}
