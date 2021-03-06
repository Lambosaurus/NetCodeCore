using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Volatile;

using NetCode;
using NetCode.Connection;
using NetCode.Connection.UDP;
using NetCode.SyncPool;

using NetcodeTest.Entities;
using NetcodeTest.Events;
using NetcodeTest.Util;
using NetcodeTest.Requests;

namespace NetcodeTest.Server
{
    public class AsteroidServer
    {
        public const bool NetworkCompression = false;

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
        private NetDefinitions NetDefs;
        private UDPServer Server;

        private Random Random;
        private Vector2 BoundaryMargin = new Vector2(30, 30);
        private Vector2 Boundary;
        private List<Physical> Physicals;
        private List<Projectile> Projectiles;
        private VoltWorld CollisionWorld;
        private double LastTimestamp;

        private ContextToken Context;

        private ServerReport serverReport = new ServerReport();

        public AsteroidServer(NetDefinitions netdefs, Vector2 boundary, int port )
        {
            NetDefs = netdefs;

            Boundary = boundary;
            Server = new UDPServer(port);
            Server.Compression = NetworkCompression;
            MaxPlayers = 8;

            OutgoingPool = new OutgoingSyncPool(netdefs, 0);
            Physicals = new List<Physical>();
            Projectiles = new List<Projectile>();
            CollisionWorld = new VoltWorld(0,1.0f);
            
            Clients = new List<RemoteClient>();

            Context = new ContextToken(this);

            int k = 4;
            for (int i = 0; i < 30*k; i++) { AddEntity(NewAsteroid(32)); }
            for (int i = 0; i < 40*k; i++) { AddEntity(NewAsteroid(48)); }
            for (int i = 0; i < 10*k; i++) { AddEntity(NewAsteroid(56)); }

            OutgoingPool.AddEntity(serverReport);

            LastTimestamp = 0;
            Random = new Random((int)System.DateTime.UtcNow.ToBinary());
        }

        private Color RandomNiceColor()
        {
            Color[] colors = new Color[]
                {
                    Color.LimeGreen,
                    Color.Red,
                    Color.Yellow,
                    Color.CornflowerBlue,
                    Color.Violet,
                    Color.Lime,
                    Color.Orange,
                    Color.Orchid,
                    Color.SeaGreen,
                    Color.OrangeRed,
                    Color.RoyalBlue,
                };
            return colors[Random.Next(colors.Length)];
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
            //ListStreaming
            //serverReport.Entities.Add(entity);
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
            OutgoingPool.AddEntity(entity);
        }

        private void AddEvent(Event evt)
        {
            OutgoingPool.AddEvent(evt);
        }
        
        private void RemoveEntity(Entity entity)
        {
            //ListStreaming
            //serverReport.Entities.Remove(entity);
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

            NetworkConnection connection = Server.RecieveConnection();
            if (connection != null)
            {
                NetworkClient client = new NetworkClient(connection);
                client.SetState(NetworkClient.ConnectionState.Open);
                client.Attach(OutgoingPool);
                IncomingSyncPool incoming = new IncomingSyncPool(NetDefs, 0);
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

                foreach (SyncEvent evt in client.Incoming.Events)
                {
                    if (evt.Obj is PlayerRequest request)
                    {
                        switch (request.Request)
                        {
                            case PlayerRequest.RequestType.FireMissile:
                                client.Player?.FireMissile();
                                break;
                            case PlayerRequest.RequestType.FireMultiMissile:
                                client.Player?.FireMultiMissile();
                                break;
                        }
                    }
                    evt.Clear();
                }

                foreach (SyncHandle handle in client.Incoming.Handles)
                {
                    if (handle.Obj is PlayerControl control)
                    {
                        if (control.Ready)
                        {
                            if (client.Player == null)
                            {
                                client.Player = NewPlayer(RandomNiceColor());
                                AddEntity(client.Player);
                                client.PlayerName = control.PlayerName;
                                serverReport.Clients.Add(control.PlayerName);
                                serverReport.Ships.Add(client.Player);
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
                        RemoveEntity(client.Player);
                    }

                    int k = serverReport.Ships.IndexOf(client.Player);
                    serverReport.Ships.RemoveAt(k);
                    serverReport.Clients.RemoveAt(k);
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

        public List<Physical> GetPhysicalsInCircle(Vector2 center, float radius)
        {
            List<Physical> Matches = new List<Physical>();
            foreach (VoltBody body in CollisionWorld.QueryCircle(center, radius))
            {
                Matches.Add((Physical)body.UserData);
            }
            return Matches;
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
