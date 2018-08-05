using System;
using System.Collections.Generic;
using System.Linq;

using NetCode;
using NetCode.Connection;
using NetCode.Connection.UDP;
using NetCode.SyncPool;

using Microsoft.Xna.Framework;

using NetcodeTest.Entities;
using NetcodeTest.Physics;

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
        
        struct RemoteClient
        {
            public NetworkClient Client;
            public IncomingSyncPool Incoming;
        }

        private float TransmitRate = 1f / 20;
        private List<RemoteClient> Clients;
        private OutgoingSyncPool OugoingPool;
        private Vector2 Boundary = new Vector2(800,600);

        NetCodeManager NetManager;

        private List<Physical> Entities;

        public AsteroidServer(NetCodeManager manager, int port )
        {
            NetManager = manager;

            Server = new UDPServer(port);
            MaxPlayers = 8;

            OugoingPool = manager.GenerateOutgoingPool(0);
            Entities = new List<Physical>();

            Clients = new List<RemoteClient>();

            for (int i = 0; i < 100; i++)
            {
                AddPhysical(NewAsteroid());
            }
        }

        private Asteroid NewAsteroid()
        {
            return new Asteroid(
                Util.RandomVector(Boundary),
                Util.CosSin(Util.RandAngle(), Util.RandF(50)),
                4 + Util.RandF(8f),
                Util.RandAngle(),
                Util.RandF(1f)
                );
        }

        private void AddPhysical(Physical entity)
        {
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
                Clients.Add(new RemoteClient() {
                    Client = client,
                    Incoming = NetManager.GenerateIncomingPool(0)
                });
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
                client.Incoming.Synchronise();
                client.Client.Update();
            }
        }
        
        private void UpdateEntitites(float delta)
        {
            long timestamp = NetTime.Now();

            foreach (Entity entity in Entities)
            {
                entity.Update(delta);
            }
            
            int end = Entities.Count;
            for (int i = 0; i < end; i++)
            {
                for (int j = i + 1; j < end; j++)
                {
                    Entities[i].HitCheck(Entities[j]);
                }
            }

            foreach (Entity entity in Entities)
            {
                if (entity.MotionUpdateRequired)
                {
                    entity.UpdateMotion(timestamp);
                }
            }
        }
    }
}
