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
        static Texture2D Triangle;
        static Texture2D Bullet;
        static Texture2D Circle;

        public static void Load(ContentManager content)
        {
            Square = content.Load<Texture2D>("sq");
            Triangle = content.Load<Texture2D>("tri");
            Circle = content.Load<Texture2D>("cir");
            Bullet = content.Load<Texture2D>("bullet");
        }

        public static void DrawSquare( SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color )
        {
            batch.Draw(Square, position, null, color, angle, new Vector2(11f, 11f), scale/20f, SpriteEffects.None, 0);
        }

        public static void DrawTriangle( SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color )
        {
            batch.Draw(Triangle, position, null, color, angle, new Vector2(7.667f, 11f), scale / 20f, SpriteEffects.None, 0);
        }

        public static void DrawCircle(SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color)
        {
            batch.Draw(Circle, position, null, color, angle, new Vector2(15f, 15f), scale / 28f, SpriteEffects.None, 0);
        }

        public static void DrawBullet(SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color)
        {
            batch.Draw(Bullet, position, null, color, angle, new Vector2(10f, 4f), new Vector2( scale.X/18f, scale.Y/6f ), SpriteEffects.None, 0);
        }
    }
}
