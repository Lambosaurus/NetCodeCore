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


namespace NetcodeTest
{
    public class TestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        NetCodeManager netcode = new NetCodeManager();
        AsteroidServer server;

        NetworkClient client;
        IncomingSyncPool incomingPool;
        
        SpriteFont font;
        
        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.AllowUserResizing = true;

            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }

        private void SetupNetwork()
        {
            NetcodeFieldSupport.RegisterCustomFields(netcode);

            netcode.RegisterType(typeof(Asteroid));
            //netcode.RegisterType(typeof(BulletEntity));

            server = new AsteroidServer(netcode, 11002);

            client = new NetworkClient(new UDPConnection(System.Net.IPAddress.Parse("127.0.0.1"), 11002, 11003));
            //client = new NetworkClient(new UDPConnection(System.Net.IPAddress.Parse("192.168.1.151"), 11002, 11003));
            incomingPool = netcode.GenerateIncomingPool(0);
            client.Attach(incomingPool);

            client.SetState(NetworkClient.ConnectionState.Open);


            NetTime.Realtime = false;
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
            NetTime.Advance( (int)(gameTime.ElapsedGameTime.TotalMilliseconds) );

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                this.Exit();
            }

            if (server != null)
            {
                server.Update(delta);
            }

            tickCounter += delta;
            if (tickCounter >= 1f/20)
            {
                tickCounter -= 1f / 20;
            }

            client.Update();
            incomingPool.Synchronise();
            
            KeyboardState keys = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            

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
            
            spriteBatch.DrawString(font, GetConnectionStatsString(client), new Vector2(0, 0), Color.White);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
