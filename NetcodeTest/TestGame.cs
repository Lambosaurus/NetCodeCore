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

        NetworkClient outgoingClient;
        NetworkClient incomingClient;

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

        private void SetupNetwork()
        {
            NetcodeFieldSupport.RegisterCustomFields(netcode);

            netcode.RegisterType(typeof(PlayerEntity));
            netcode.RegisterType(typeof(BulletEntity));
            
            outgoingClient = new NetworkClient( new UDPConnection( System.Net.IPAddress.Parse("127.0.0.1"), 11003, 11002 ));
            incomingClient = new NetworkClient( new UDPConnection( System.Net.IPAddress.Parse("127.0.0.1"), 11002, 11003 ));

            outgoingPool = netcode.GenerateOutgoingPool(1);
            incomingPool = netcode.GenerateIncomingPool(1);
            outgoingClient.Attach(outgoingPool);
            incomingClient.Attach(incomingPool);

            outgoingPool.RegisterEntity(player);

            NetTime.Realtime = false;
        }

        KeyboardState lastKeys;
        MouseState lastMouse;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Entity.Load(Content);

            font = Content.Load<SpriteFont>("MenuFont");

            SetupNetwork();

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
            NetTime.Advance( (int)(gameTime.ElapsedGameTime.TotalMilliseconds) );

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            KeyboardState keys = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            Vector2 control = new Vector2(0,0);
            if (keys.IsKeyDown(Keys.S))
            {
                control.Y += 1;
            }
            if (keys.IsKeyDown(Keys.W))
            {
                control.Y -= 1;
            }
            if (keys.IsKeyDown(Keys.D))
            {
                control.X += 1;
            }
            if (keys.IsKeyDown(Keys.A))
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
                    fireTick = 2;
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

            if (keys.IsKeyDown(Keys.C) && !lastKeys.IsKeyDown(Keys.C))
            {
                if (outgoingClient.State != NetworkClient.ConnectionState.Closed)
                {
                    outgoingClient.SetState(NetworkClient.ConnectionState.Closed);
                }
                else
                {
                    outgoingClient.SetState(NetworkClient.ConnectionState.Open);
                }
            }
            

            if (++tick >= 5)
            {
                tick = 0;
                outgoingPool.Synchronise();
            }

            outgoingClient.Update();
            incomingClient.Update();
            incomingPool.Synchronise();


            if (incomingClient.State == NetworkClient.ConnectionState.Closed)
            {
                incomingClient.SetState(NetworkClient.ConnectionState.Listening);
            }


            lastKeys = keys;
            lastMouse = mouse;
            base.Update(gameTime);
        }

        private string GetConnectionStatsString(NetworkClient client)
        {
            ConnectionStats stats = client.Connection.Stats;
            string text = string.Format(
                "up: {0:0.00}KB/s\ndown: {1:0.00}KB/s\nping: {2}ms\nloss: {3}%\ntimeout: {4:0.0}s\nstate: {5}",
                stats.SentBytesPerSecond / 1024.0,
                stats.RecievedBytesPerSecond / 1024.0,
                stats.Latency,
                (int)(stats.PacketLoss * 100),
                ((double)stats.MillisecondsSinceLastAcknowledgement)/1000,
                client.State.ToString()
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

            spriteBatch.DrawString(font, GetConnectionStatsString(outgoingClient), new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, GetConnectionStatsString(incomingClient), new Vector2(0, 300), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
