using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using NetCode;

namespace NetcodeTest
{
    public class Entity
    {
        public enum ColorNo { Red, Green, Blue };
        static Color[] colortable = new Color[] { Color.Red, Color.Green, Color.Blue };

        [Synchronisable]
        public ColorNo color;
        [Synchronisable]
        public Vector2 position;
        
        public Vector2 velocity;
        
        public Entity()
        {
            color = ColorNo.Red;
        }

        public Entity(Vector2 starting_location, ColorNo arg_color)
        {
            position = starting_location;
            color = arg_color;
        }
        
        public void Update()
        {
            position += velocity;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(sq_texture, position, null, colortable[(int)color], 0f, sq_center, 1f, SpriteEffects.None, 0);
        }
        
        static Texture2D sq_texture;
        static Vector2 sq_center;
        public static void Load(ContentManager content)
        {
            sq_texture = content.Load<Texture2D>("sq");
            sq_center = new Vector2(sq_texture.Width, sq_texture.Height) / 2f;
        }
    }
}
