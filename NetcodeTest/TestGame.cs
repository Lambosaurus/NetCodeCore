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
using NetcodeTest.Requests;

namespace NetcodeTest
{
    public class TestGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        NetDefinitions netDefs;
        AsteroidServer server = null;

        NetworkClient client;
        IncomingSyncPool incomingPool;
        OutgoingSyncPool outgoingPool;
        PlayerControl controlVector;

        List<Event> events = new List<Event>();

        Point Resolution = new Point(1200, 800);

        Plotter networkPlot;
        public TestGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.AllowUserResizing = false;
            graphics.PreferredBackBufferWidth = Resolution.X;
            graphics.PreferredBackBufferHeight = Resolution.Y;
            
            IsMouseVisible = true;

            networkPlot = new Plotter(30, 10, new Vector2(400, 200))
            {
                Origin = new Vector2(200, 10),
                Unit = "KB/s",
                TracePointSize = 6,
                TraceThickness = 4,
                AutoScaleUp = true,
                AutoScaleDown = true,
            };
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }

        private void SetupNetwork()
        {
            netDefs = new NetDefinitions();
            server = new AsteroidServer(netDefs, Resolution.ToVector2(), 11002);
            
            client = new NetworkClient( new UDPConnection(
                System.Net.IPAddress.Parse( (server != null) ? "127.0.0.1" : "122.58.99.13"),
                11002, // Change this destination to 12002 to connect to a running NetProxy.
                (server != null) ? 11003 : 11002
                ));
            client.Connection.Stats.ByteAggregationPeriodMilliseconds = 100;

            // Note, when using NetProxy with this setup, the Open command should be:
            // open 12002 11003 12003 11002

            incomingPool = new IncomingSyncPool(netDefs, 0);
            outgoingPool = new OutgoingSyncPool(netDefs, 0);
            client.Attach(incomingPool);
            client.Attach(outgoingPool);
            controlVector = new PlayerControl()
            {
                Ready = true,
                PlayerName = System.Environment.MachineName
            };
            outgoingPool.AddEntity(controlVector);

            client.SetState(NetworkClient.ConnectionState.Open);
        }

        KeyboardState lastKeys;
        MouseState lastMouse;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Drawing.Load(Content);

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
            //NetTime.Realtime = false;
            //NetTime.Advance(1000/60);

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
            
            if (keys.IsKeyDown(Keys.Q) && !lastKeys.IsKeyDown(Keys.Q))
            {
                outgoingPool.AddEvent(new PlayerRequest(PlayerRequest.RequestType.FireMissile));
            }

            tickCounter += delta;
            if (tickCounter >= 1f / 20)
            {
                tickCounter -= 1f / 20;
                outgoingPool.Synchronise();
            }

            client.Update();
            incomingPool.Synchronise();

            networkPlot.AddValue(client.Connection.Stats.BytesRecieved.PerSecond / 1024);


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

            ServerReport serverReport = null;

            foreach ( SyncHandle handle in incomingPool.Handles )
            {
                
                if (handle.Obj is Entity entity)
                {
                    entity.Predict(timestamp);
                    entity.Draw(spriteBatch);
                }
                
                if (handle.Obj is ServerReport report)
                {
                    serverReport = report;
                    
                    //ListStreaming
                    //if (report.Entities != null)
                    //{
                    //    foreach (Entity entity in report.Entities)
                    //    {
                    //        if (entity != null)
                    //        {
                    //            entity.Predict(timestamp);
                    //            entity.Draw(spriteBatch);
                    //        }
                    //    }
                    //}
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

            networkPlot.Draw(spriteBatch);

            Drawing.DrawHardSquare(spriteBatch, new Vector2(100, 150), new Vector2(200, 300), 0, Color.Black * 0.5f);

            if (serverReport != null)
            {
                for (int i = 0; i < serverReport.Clients.Count; i++)
                {
                    Color color = Color.White;
                    if (serverReport.Ships[i] != null) { color = serverReport.Ships[i].Color; }
                    Drawing.DrawString(spriteBatch, serverReport.Clients[i], new Vector2(10, 200 + i * 24), 24, color);
                }
            }

            Drawing.DrawString(spriteBatch, GetConnectionStatsString(client), new Vector2(10, 10), 24, Color.White);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
