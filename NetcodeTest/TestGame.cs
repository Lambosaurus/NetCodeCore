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
using NetcodeTest.Events;

namespace NetcodeTest
{
    public class TestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        NetDefinitions netDefs = new NetDefinitions();
        AsteroidServer server = null;

        NetworkClient client;
        IncomingSyncPool incomingPool;
        OutgoingSyncPool outgoingPool;
        PlayerControl controlVector;

        List<Event> events = new List<Event>();

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
            NetcodeFields.RegisterCustomFields(netDefs);

            netDefs.RegisterType(typeof(Asteroid));
            netDefs.RegisterType(typeof(Ship));
            netDefs.RegisterType(typeof(PlayerControl));
            netDefs.RegisterType(typeof(Projectile));
            netDefs.RegisterType(typeof(Explosion));
            netDefs.RegisterType(typeof(ServerReport));

            server = new AsteroidServer(netDefs, Resolution.ToVector2(), 11002);
            
            client = new NetworkClient( new UDPConnection(
                System.Net.IPAddress.Parse( (server != null) ? "127.0.0.1" : "122.61.155.237"),
                11002,
                (server != null) ? 11003 : 11002
                ));

            incomingPool = netDefs.GenerateIncomingPool(0);
            outgoingPool = netDefs.GenerateOutgoingPool(0);
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
            NetTime.Realtime = false;
            NetTime.Advance(1000/60);

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
                if (handle.Obj is ServerReport report)
                {
                    for (int i = 0; i < report.Clients.Count; i++)
                    {
                        Color color = Color.White;
                        if (report.Ships[i] != null) { color = report.Ships[i].Color; }
                        spriteBatch.DrawString(font, report.Clients[i], new Vector2(0, 200 + i * 12), color);
                    }
                }
            }


            foreach (SyncEvent syncEvent in incomingPool.Events)
            {
                if (syncEvent.Obj is Event evt)
                {
                    evt.Predict(timestamp);
                    if (evt.Expired())
                    {
                        syncEvent.Clear();
                    }
                    else
                    {
                        evt.Draw(spriteBatch);
                    }
                }
                else
                {
                    syncEvent.Clear();
                }
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
