using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace NetcodeTest
{
    public static class Drawing
    {
        static Texture2D Square;

        public static void Load(ContentManager content)
        {
            Square = content.Load<Texture2D>("sq");
        }

        public static void DrawSquare( SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color )
        {
            batch.Draw(Square, position, null, color, angle, new Vector2(6f, 6f), scale/5, SpriteEffects.None, 0);
        }
    }
}
