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
using NetCode.Util;
using NetCode.SyncPool;
using NetCode.Connection;
using NetCode.Connection.UDP;

using NetcodeTest.Entities;
using NetcodeTest.Server;
using NetcodeTest.Util;

namespace NetcodeTest
{
    public class TestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        NetDefinition netcode = new NetDefinition();
        AsteroidServer server = null;

        NetworkClient client;
        IncomingSyncPool incomingPool;
        OutgoingSyncPool outgoingPool;
        PlayerControl controlVector;
        
        SpriteFont font;
        
        Point Resolution = new Point(1200, 800);
        
        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.AllowUserResizing = false;
            graphics.PreferredBackBufferWidth = Resolution.X;
            graphics.PreferredBackBufferHeight = Resolution.Y;
            
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }

        private void SetupNetwork()
        {
            NetcodeFields.RegisterCustomFields(netcode);

            netcode.RegisterType(typeof(Asteroid));
            netcode.RegisterType(typeof(Ship));
            netcode.RegisterType(typeof(PlayerControl));
            netcode.RegisterType(typeof(Projectile));

            server = new AsteroidServer(netcode, Resolution.ToVector2(), 11002);
            
            client = new NetworkClient( new UDPConnection(
                System.Net.IPAddress.Parse( (server != null) ? "127.0.0.1" : "122.58.86.5"),
                11002,
                (server != null) ? 11003 : 11002
                ));

            incomingPool = netcode.GenerateIncomingPool(0);
            outgoingPool = netcode.GenerateOutgoingPool(0);
            client.Attach(incomingPool);
            client.Attach(outgoingPool);
            controlVector = new PlayerControl()
            {
                ShipColor = Color.Red,
                Ready = true,
                PlayerName = "Lambosaurus"
            };
            outgoingPool.RegisterEntity(controlVector);

            client.SetState(NetworkClient.ConnectionState.Open);
        }

        KeyboardState lastKeys;
        MouseState lastMouse;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Drawing.Load(Content);

            font = Content.Load<SpriteFont>("MenuFont");

            SetupNetwork();

            lastKeys = Keyboard.GetState();
            lastMouse = Mouse.GetState();
        }

        protected override void UnloadContent()
        {
        }

        float tickCounter = 0;

        protected override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if (server != null)
            {
                server.Update();
            }

            
            KeyboardState keys = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            controlVector.Thrust = (keys.IsKeyDown(Keys.W) ? 1.0f : 0f);
            controlVector.Torque = (keys.IsKeyDown(Keys.A) ? 1.0f : 0f) + (keys.IsKeyDown(Keys.D) ? -1.0f : 0f);
            controlVector.Firing = keys.IsKeyDown(Keys.Space); // Should be done as events.
            
            tickCounter += delta;
            if (tickCounter >= 1f / 20)
            {
                tickCounter -= 1f / 20;
                outgoingPool.Synchronise();
            }

            client.Update();
            incomingPool.Synchronise();

            lastKeys = keys;
            lastMouse = mouse;
            base.Update(gameTime);
        }

        private string GetConnectionStatsString(NetworkClient client)
        {
            ConnectionStats stats = client.Connection.Stats;
            string text = string.Format(
                "up: {0}\ndown: {1}\nping: {2}ms\nloss: {3}%\ntimeout: {4:0.0}s\nstate: {5}\noffset: {6}ms",
                Primitive.SIFormat( stats.BytesSent.PerSecond, "B/s"),
                Primitive.SIFormat( stats.BytesRecieved.PerSecond, "B/s"),
                stats.Latency,
                (int)(stats.PacketLoss * 100),
                ((double)stats.MillisecondsSinceAcknowledgement)/1000,
                client.State.ToString(),
                stats.NetTimeOffset
                );
            return text;
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(samplerState:SamplerState.LinearClamp);

            long timestamp = NetTime.Now();
            
            foreach ( SyncHandle handle in incomingPool.Handles )
            {
                if (handle.Obj is Entity entity)
                {
                    entity.Predict(timestamp);
                    entity.Draw(spriteBatch);
                }
            }
            
            if (server != null)
            {
                string[] clients = server.GetClientInfo();
                string packed = string.Join("\n", clients);
                spriteBatch.DrawString(font, packed, new Vector2(0, 200), Color.White);
            }
            
            /*
            foreach ( Entity entity in server.Entities )
            {
                entity.Draw(spriteBatch);
            }
            */
            
            spriteBatch.DrawString(font, GetConnectionStatsString(client), new Vector2(0, 0), Color.White);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
