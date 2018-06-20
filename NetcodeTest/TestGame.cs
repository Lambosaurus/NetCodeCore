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

using NetcodeTest.Entities;

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

        PlayerEntity player;
        List<Entity> entities;

        SpriteFont font;
        
        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            player = new PlayerEntity( new Vector2(200,200), PlayerEntity.ColorNo.Blue);
            entities = new List<Entity>();
            entities.Add(player);
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }

        KeyboardState lastKeys;
        MouseState lastMouse;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Entity.Load(Content);

            font = Content.Load<SpriteFont>("MenuFont");

            NetcodeFieldSupport.RegisterCustomFields(netcode);

            netcode.RegisterType(typeof(PlayerEntity));
            netcode.RegisterType(typeof(BulletEntity));
            
            outgoingConnection = new VirtualConnection();
            incomingConnection = new VirtualConnection();
            outgoingConnection.Connect(incomingConnection);

            outgoingConnection.Settings.PacketLoss = 0.0;
            outgoingConnection.Settings.LatencyMin = 0;
            outgoingConnection.Settings.LatencyMax = 500;

            outgoingPool = netcode.GenerateOutgoingPool(1);
            incomingPool = netcode.GenerateIncomingPool(1);
            outgoingPool.AddDestination(outgoingConnection);
            incomingPool.SetSource(incomingConnection);
            
            outgoingPool.RegisterEntity(player);

            lastKeys = Keyboard.GetState();
            lastMouse = Mouse.GetState();
        }

        protected override void UnloadContent()
        {
        }

        int tick = 0;
        int fireTick = 0;
        

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            KeyboardState keys = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

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
                player.color++;
                if (player.color > (PlayerEntity.ColorNo)2)
                {
                    player.color = (PlayerEntity.ColorNo)0;
                }
            }

            player.velocity = control;
            
            if (fireTick > 0)
            {
                fireTick--;
            }
            else
            {
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    fireTick = 10;
                    double angle = Util.AngleTo(player.position, mouse.Position.ToVector2());
                    BulletEntity bullet = new BulletEntity(player.position, angle);
                    entities.Add(bullet);
                    outgoingPool.RegisterEntity(bullet);
                }
            }
            
            foreach (Entity entity in entities)
            {
                entity.Update();
            }

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].expired)
                {
                    outgoingPool.GetHandleByObject(entities[i]).State = SyncHandle.SyncState.Deleted;
                    entities.RemoveAt(i);
                }
            }
            

            if (++tick >= 5)
            {
                tick = 0;
                outgoingPool.Synchronise();
            }

            outgoingConnection.Update();
            incomingConnection.Update();
            incomingPool.Synchronise();

            lastKeys = keys;
            lastMouse = mouse;
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
            

            foreach (Entity entity in entities)
            {
                entity.Draw(spriteBatch, true);
            }

            foreach (SyncHandle handle in incomingPool.Handles)
            {
                Entity entity = (Entity)(handle.Obj);
                entity.Draw(spriteBatch, false);
            }

            spriteBatch.DrawString(font, GetConnectionStatsString(outgoingConnection.Stats), new Vector2(0, 0), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
