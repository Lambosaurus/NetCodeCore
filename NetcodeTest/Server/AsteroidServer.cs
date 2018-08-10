using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using NetCode;
using NetCode.Connection;
using NetCode.Connection.UDP;
using NetCode.SyncPool;
using Microsoft.Xna.Framework;
using Volatile;

using NetcodeTest.Entities;
using NetcodeTest.Util;


namespace NetcodeTest.Server
{
    public class AsteroidServer
    {
        
        public int MaxPlayers
        {
            get { return Server.IncomingConnectionLimit; }
            set { Server.IncomingConnectionLimit = value; }
        }
        
        class RemoteClient
        {
            public NetworkClient Client;
            public IncomingSyncPool Incoming;
            public Ship Player;
            public string PlayerName = "";
        }

        private float TransmitRate = 1f / 20;
        private List<RemoteClient> Clients;
        private OutgoingSyncPool OutgoingPool;
        private NetCodeManager NetManager;
        private UDPServer Server;
        
        private Vector2 BoundaryMargin = new Vector2(20, 20);
        private Vector2 Boundary;
        private List<Entity> Entities;
        private VoltWorld CollisionWorld;
        private double LastTimestamp;

        public AsteroidServer(NetCodeManager manager, Vector2 boundary, int port )
        {
            NetManager = manager;

            Boundary = boundary;
            Server = new UDPServer(port);
            MaxPlayers = 8;

            OutgoingPool = manager.GenerateOutgoingPool(0);
            Entities = new List<Entity>();
            CollisionWorld = new VoltWorld(0,1.0f);
            
            Clients = new List<RemoteClient>();

            for (int i = 0; i < 50; i++) { AddEntity(NewAsteroid(16)); }
            for (int i = 0; i < 60; i++) { AddEntity(NewAsteroid(24)); }
            for (int i = 0; i < 20; i++) { AddEntity(NewAsteroid(32)); }
            for (int i = 0; i < 5; i++) { AddEntity(NewAsteroid(48)); }

            LastTimestamp = NetTime.Seconds();
        }

        private Asteroid NewAsteroid(float scale)
        {
            return new Asteroid(
                Fmath.RandomVector(Boundary),
                Fmath.CosSin(Fmath.RandAngle(), Fmath.RandF(50)),
                scale/2 + Fmath.RandF(scale/2),
                Fmath.RandAngle(),
                Fmath.RandF(1f)
                );
        }

        private Ship NewPlayer(Color color)
        {
            return new Ship(
                Fmath.RandomVector(Boundary/2) + (Boundary/4),
                Fmath.CosSin(Fmath.RandAngle(), Fmath.RandF(50)),
                color,
                Fmath.RandAngle(),
                Fmath.RandF(1f)
                );
        }

        private void AddEntity(Entity entity)
        {
            entity.GenerateBody(CollisionWorld);
            entity.UpdateMotion(NetTime.Now());
            Entities.Add(entity);
            
            OutgoingPool.RegisterEntity(entity);
        }

        private void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
            OutgoingPool.GetHandleByObject(entity).State = SyncHandle.SyncState.Deleted;
            entity.DestroyBody();
        }
        

        float syncCounter = 0;
        
        public void Update()
        {
            double seconds = NetTime.Seconds();
            float delta = (float)(seconds - LastTimestamp);
            LastTimestamp = seconds;

            UDPFeed feed = Server.RecieveConnection();
            if (feed != null)
            {
                NetworkClient client = new NetworkClient(feed);
                client.SetState(NetworkClient.ConnectionState.Open);
                client.Attach(OutgoingPool);
                IncomingSyncPool incoming = NetManager.GenerateIncomingPool(0);
                client.Attach(incoming);

                RemoteClient newClient = new RemoteClient() {
                    Client = client,
                    Incoming = incoming,
                    Player = null
                };

                Clients.Add(newClient);
            }
            UpdateEntitites(delta);
            
            syncCounter += delta;
            if (syncCounter >= TransmitRate)
            {
                syncCounter -= TransmitRate;

                OutgoingPool.Synchronise();
            }

            foreach ( RemoteClient client in Clients )
            {
                client.Client.Update();
                client.Incoming.Synchronise();

                foreach (SyncHandle handle in client.Incoming.Handles)
                {
                    if (handle.Obj is PlayerControl control)
                    {
                        if (control.Ready)
                        {
                            if (client.Player == null)
                            {
                                client.Player = NewPlayer(control.ShipColor);
                                AddEntity(client.Player);
                                client.PlayerName = control.PlayerName;
                            }
                            client.Player.Control(control.Thrust, control.Torque);
                        }

                        break;
                    }
                }
            }

            for (int i = Clients.Count-1; i >= 0; i--)
            {
                RemoteClient client = Clients[i];
                if (client.Client.State == NetworkClient.ConnectionState.Closed)
                {
                    Clients.RemoveAt(i);
                    client.Client.Destroy();
                    if (client.Player != null)
                    {
                        RemoveEntity(client.Player);
                    }
                }
            }
        }

        public string[] GetClientInfo()
        {
            string[] lines = new string[Clients.Count];
            for (int i = 0; i < Clients.Count; i++)
            {
                RemoteClient client = Clients[i];
                if (client.Client.State != NetworkClient.ConnectionState.Open)
                {
                    lines[i] = client.Client.State.ToString();
                }
                else
                {
                    lines[i] = string.Format("{0}: {1}ms", client.PlayerName, client.Client.Connection.Stats.Latency);
                }
            }
            return lines;
        }
        
        private void UpdateEntitites(float delta)
        {
            long timestamp = NetTime.Now();

            CollisionWorld.DeltaTime = delta;
            CollisionWorld.Update();


            Vector2 MarginLow = - BoundaryMargin;
            Vector2 MarginHigh = Boundary + BoundaryMargin;

            foreach (Entity entity in Entities)
            {
                entity.Update(delta);
                entity.Clamp(MarginLow, MarginHigh);

                if (entity.MotionRequestRequired)
                {
                    entity.UpdateMotion(timestamp);
                }
            }
        }
    }
}
