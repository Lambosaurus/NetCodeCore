using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using NetCode;
using NetCode.SyncPool;

namespace NetcodeTest
{
    public class TestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        NetCodeManager netcode = new NetCodeManager();
        OutgoingSyncPool outgoingPool;
        IncomingSyncPool incmomingPool;

        Entity entity;
        
        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            entity = new Entity( new Vector2(200,200), Entity.ColorNo.Blue);
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Entity.Load(Content);

            netcode.RegisterField(typeof(SynchronisableVector2), typeof(Vector2));
            netcode.RegisterField(typeof(SynchronisableHalfVector2), typeof(Vector2), SyncFlags.HalfPrecisionFloats);

            netcode.RegisterType(typeof(Entity));
            outgoingPool = netcode.GenerateOutgoingPool(1);
            incmomingPool = netcode.GenerateIncomingPool(1);

            outgoingPool.RegisterEntity(entity);
        }

        protected override void UnloadContent()
        {
        }

        int tick = 0;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            KeyboardState keys = Keyboard.GetState();

            Vector2 control = new Vector2(0,0);
            if (keys.IsKeyDown(Keys.Down))
            {
                control.Y += 1;
            }
            if (keys.IsKeyDown(Keys.Up))
            {
                control.Y -= 1;
            }
            if (keys.IsKeyDown(Keys.Right))
            {
                control.X += 1;
            }
            if (keys.IsKeyDown(Keys.Left))
            {
                control.X -= 1;
            }

            entity.velocity = control;

            entity.Update();

            if (++tick >= 3)
            {
                tick = 0;
                outgoingPool.UpdateFromLocal();
                outgoingPool.GenerateDeltaPacket(1);
                
                // Send packet here //
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            entity.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
