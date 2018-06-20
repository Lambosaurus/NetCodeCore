using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using NetCode;

namespace NetcodeTest.Entities
{
    public class Entity
    {
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public Vector2 position;
        
        public Vector2 velocity;

        public bool expired = false;
        
        public virtual void Update()
        {
            position += velocity;
        }

        public virtual void Draw(SpriteBatch batch, bool local)
        {
        }
        
        protected static Texture2D sq_texture;
        protected static Vector2 sq_center;
        public static void Load(ContentManager content)
        {
            sq_texture = content.Load<Texture2D>("sq");
            sq_center = new Vector2(sq_texture.Width, sq_texture.Height) / 2f;
        }
    }
}
