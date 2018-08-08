using System;
using System.Collections.Generic;
using System.Linq;

using NetCode;
using NetCode.Connection;
using NetCode.Connection.UDP;
using NetCode.SyncPool;

using Microsoft.Xna.Framework;
using Volatile;

using NetcodeTest.Entities;


namespace NetcodeTest.Server
{
    public class AsteroidServer
    {

        UDPServer Server;
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
        }

        private float TransmitRate = 1f / 20;
        private List<RemoteClient> Clients;
        private OutgoingSyncPool OugoingPool;

        private Vector2 BoundaryMargin = new Vector2(20, 20);
        private Vector2 Boundary;

        NetCodeManager NetManager;

        public List<Entity> Entities;

        public VoltWorld CollisionWorld;

        public AsteroidServer(NetCodeManager manager, Vector2 boundary, int port )
        {
            NetManager = manager;

            Boundary = boundary;
            Server = new UDPServer(port);
            MaxPlayers = 8;

            OugoingPool = manager.GenerateOutgoingPool(0);
            Entities = new List<Entity>();
            CollisionWorld = new VoltWorld(0,1.0f);
            
            Clients = new List<RemoteClient>();

            for (int i = 0; i < 100; i++)
            {
                AddEntity(NewAsteroid());
            }
        }

        private Asteroid NewAsteroid()
        {
            return new Asteroid(
                Util.RandomVector(Boundary),
                Util.CosSin(Util.RandAngle(), Util.RandF(50)),
                8 + Util.RandF(16f),
                Util.RandAngle(),
                Util.RandF(1f)
                );
        }

        private Ship NewPlayer(Color color)
        {
            return new Ship(
                Util.RandomVector(Boundary/2) + (Boundary/4),
                Util.CosSin(Util.RandAngle(), Util.RandF(50)),
                color,
                Util.RandAngle(),
                Util.RandF(1f)
                );
        }

        private void AddEntity(Entity entity)
        {
            entity.GenerateBody(CollisionWorld);
            entity.UpdateMotion(NetTime.Now());
            Entities.Add(entity);
            
            OugoingPool.RegisterEntity(entity);
        }


        float syncCounter = 0;
        
        public void Update(float delta)
        {
            UDPFeed feed = Server.RecieveConnection();
            if (feed != null)
            {
                NetworkClient client = new NetworkClient(feed);
                client.SetState(NetworkClient.ConnectionState.Open);
                client.Attach(OugoingPool);
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

                OugoingPool.Synchronise();
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
                            }
                            client.Player.Control(control.Thrust, control.Torque);
                        }

                        break;
                    }
                }
            }
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
