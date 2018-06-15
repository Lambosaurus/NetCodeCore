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
using NetCode.Connection;

namespace NetcodeTest
{
    public class TestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        NetCodeManager netcode = new NetCodeManager();
        OutgoingSyncPool outgoingPool;
        IncomingSyncPool incomingPool;
        VirtualConnection outgoingConnection;
        VirtualConnection incomingConnection;

        Entity entity;

        SpriteFont font;
        
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

        KeyboardState lastKeys;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Entity.Load(Content);

            font = Content.Load<SpriteFont>("MenuFont");

            NetcodeFieldSupport.RegisterCustomFields(netcode);

            netcode.RegisterType(typeof(Entity));
            
            outgoingConnection = new VirtualConnection();
            incomingConnection = new VirtualConnection();
            outgoingConnection.Connect(incomingConnection);

            outgoingConnection.Settings.PacketLoss = 0.5;
            outgoingConnection.Settings.LatencyMin = 50;
            outgoingConnection.Settings.LatencyMax = 100;

            outgoingPool = netcode.GenerateOutgoingPool(1);
            incomingPool = netcode.GenerateIncomingPool(1);
            outgoingPool.AddDestination(outgoingConnection);
            incomingPool.SetSource(incomingConnection);
            
            outgoingPool.RegisterEntity(entity);

            lastKeys = Keyboard.GetState();
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
            if (keys.IsKeyDown(Keys.Space) && !lastKeys.IsKeyDown(Keys.Space))
            {
                entity.color++;
                if (entity.color > (Entity.ColorNo)2)
                {
                    entity.color = (Entity.ColorNo)0;
                }
            }

            entity.velocity = control;

            entity.Update();

            if (++tick >= 10)
            {
                tick = 0;
                outgoingPool.Synchronise();
            }

            outgoingConnection.Update();
            incomingConnection.Update();
            incomingPool.Synchronise();

            lastKeys = keys;
            base.Update(gameTime);
        }


        private string GetConnectionStatsString(ConnectionStats stats)
        {
            string text = string.Format(
                "up: {0}B/s\ndown: {1}B/s\nping: {2}ms\nloss: {3}%",
                stats.SentBytesPerSecond,
                stats.RecievedBytesPerSecond,
                stats.Latency,
                (int)(stats.PacketLoss * 100)
                );
            return text;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            
            entity.Draw(spriteBatch);

            foreach (SyncHandle handle in incomingPool.Handles)
            {
                Entity entity = (Entity)(handle.Obj);
                entity.Draw(spriteBatch);
            }

            spriteBatch.DrawString(font, GetConnectionStatsString(outgoingConnection.Stats), new Vector2(0, 0), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
