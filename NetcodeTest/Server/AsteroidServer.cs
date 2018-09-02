using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using NetCode;
using NetCode.Connection;
using NetCode.Connection.UDP;
using NetCode.SyncPool;
using Volatile;

using NetcodeTest.Entities;
using NetcodeTest.Util;
using NetcodeTest.Events;




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
        private NetDefinition NetDefs;
        private UDPServer Server;
        
        private Vector2 BoundaryMargin = new Vector2(30, 30);
        private Vector2 Boundary;
        private List<Physical> Physicals;
        private List<Projectile> Projectiles;
        private VoltWorld CollisionWorld;
        private double LastTimestamp;

        private ContextToken Context;

        public AsteroidServer(NetDefinition netdefs, Vector2 boundary, int port )
        {
            NetDefs = netdefs;

            Boundary = boundary;
            Server = new UDPServer(port);
            MaxPlayers = 8;

            OutgoingPool = netdefs.GenerateOutgoingPool(0);
            Physicals = new List<Physical>();
            Projectiles = new List<Projectile>();
            CollisionWorld = new VoltWorld(0,1.0f);
            
            Clients = new List<RemoteClient>();

            Context = new ContextToken();

            int k = 2;
            for (int i = 0; i < 30*k; i++) { AddEntity(NewAsteroid(32)); }
            for (int i = 0; i < 40*k; i++) { AddEntity(NewAsteroid(48)); }
            for (int i = 0; i < 10*k; i++) { AddEntity(NewAsteroid(56)); }

            LastTimestamp = 0;
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
            entity.SetContext(Context);

            if (entity is Physical phys)
            {
                phys.GenerateBody(CollisionWorld);
                Physicals.Add(phys);
            }
            else if (entity is Projectile proj)
            {
                Projectiles.Add(proj);
            }
            else
            {
                throw new ArgumentException();
            }

            entity.UpdateMotion(NetTime.Now());
            OutgoingPool.RegisterEntity(entity);
        }

        private void AddEvent(Event evt)
        {
            OutgoingPool.RegisterEvent(evt);
        }
        
        private void RemoveEntity(Entity entity)
        {
            entity.OnDestroy();

            if (entity is Physical phys)
            {
                Physicals.Remove(phys);
            }
            else if (entity is Projectile proj)
            {
                Projectiles.Remove(proj);
            }
            else
            {
                throw new ArgumentException();
            }
            
            OutgoingPool.GetHandleByObject(entity).State = SyncHandle.SyncState.Deleted;
        }
        float syncCounter = 0;
        
        public void Update()
        {
            
            if (LastTimestamp == 0)
            {
                LastTimestamp = NetTime.Seconds() - 0.016;
                foreach (Physical phys in Physicals)
                {
                    phys.RequestMotionUpdate();
                }
            }
            
            double seconds = NetTime.Seconds();
            float delta = (float)(seconds - LastTimestamp);
            LastTimestamp = seconds;

            UDPFeed feed = Server.RecieveConnection();
            if (feed != null)
            {
                NetworkClient client = new NetworkClient(feed);
                client.SetState(NetworkClient.ConnectionState.Open);
                client.Attach(OutgoingPool);
                IncomingSyncPool incoming = NetDefs.GenerateIncomingPool(0);
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
                            client.Player.Control(control.Thrust, control.Torque, control.Firing);
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
                        AddEntity(client.Player);
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
        
        private void UpdateEntity(Entity entity, float delta, long timestamp)
        {
            entity.Update(delta);
            entity.Clamp(-BoundaryMargin, Boundary + BoundaryMargin);
            if (entity.NeedsMotionReset)
            {
                entity.UpdateMotion(timestamp);
            }
        }

        private void ProjectileCollision(Projectile proj)
        {
            foreach (VoltBody body in CollisionWorld.QueryPoint(proj.Position))
            {
                Physical phys = (Physical)body.UserData;
                if (phys != proj.Creator)
                {
                    proj.OnCollide((Physical)body.UserData);
                    break;
                }
            }
        }
        
        
        private void UpdateEntitites(float delta)
        {
            long timestamp = NetTime.Now();

            CollisionWorld.DeltaTime = delta;
            CollisionWorld.Update();


            foreach (Projectile proj in Projectiles)
            {
                UpdateEntity(proj, delta, timestamp);
                ProjectileCollision(proj);
            }
            foreach (Physical phys in Physicals)
            {
                UpdateEntity(phys, delta, timestamp);
            }
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                if (Projectiles[i].IsDestroyed) { RemoveEntity(Projectiles[i]); }
            }
            for (int i = Physicals.Count - 1; i >= 0; i--)
            {
                if (Physicals[i].IsDestroyed) { RemoveEntity(Physicals[i]); }
            }
            
            List<Entity> spawned = Context.GetEntities();
            if (spawned != null)
            {
                foreach (Entity entity in spawned)
                {
                    AddEntity(entity);
                }
            }

            List<Event> events = Context.GetEvents();
            if (events != null)
            {
                foreach (Event evt in events)
                {
                    AddEvent(evt);
                }
            }
        }
    }
}
