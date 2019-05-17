using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using NetcodeTest.Util;

namespace NetcodeTest
{
    public static class Drawing
    {
        static Texture2D Square;
        static Texture2D Triangle;
        static Texture2D Bullet;
        static Texture2D Circle;
        static Texture2D Box;
        static SpriteFont Font;

        public static void Load(ContentManager content)
        {
            Square = content.Load<Texture2D>("sq");
            Triangle = content.Load<Texture2D>("tri");
            Circle = content.Load<Texture2D>("cir");
            Bullet = content.Load<Texture2D>("bullet");
            Box = content.Load<Texture2D>("box");
            Font = content.Load<SpriteFont>("MenuFont");
        }

        public static void DrawSquare( SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color )
        {
            batch.Draw(Square, position, null, color, angle, new Vector2(11f, 11f), scale/20f, SpriteEffects.None, 0);
        }

        public static void DrawHardSquare(SpriteBatch batch, Vector2 position, Vector2 scale, float angle, Color color)
        {
            batch.Draw(Box, position, null, color, angle, new Vector2(11f, 11f), scale / 22f, SpriteEffects.None, 0);
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

        public static void DrawLine(SpriteBatch batch, Vector2 start, Vector2 stop, float thickness, Color color)
        {
            float length = (stop - start).Length();
            float angle = Fmath.AngleTo(start, stop);
            Vector2 center = (start + stop) / 2;
            DrawSquare(batch, center, new Vector2(length, thickness), angle, color);
        }

        public static void DrawHardLine(SpriteBatch batch, Vector2 start, Vector2 stop, float thickness, Color color)
        {
            float length = (stop - start).Length();
            float angle = Fmath.AngleTo(start, stop);
            Vector2 center = (start + stop) / 2;
            DrawHardSquare(batch, center, new Vector2(length, thickness), angle, color);
        }

        public static void DrawString(SpriteBatch batch, string text, Vector2 position, float size, Color color)
        {
            batch.DrawString(Font, text, position, color, 0, Vector2.Zero, size / 22.0f, SpriteEffects.None, 0);
        }
    }
}
