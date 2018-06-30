using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;
using NetCode.Packing;

namespace NetCode.Connection
{
    public class NetworkClient
    {
        
        public int KeepAliveTimeout { get; set; } = 300;
        public int ConnectionClosedTimeout { get; set; } = 5000;
        public NetworkConnection Connection { get; private set; }
        
        public enum ConnectionState
        {
            /// <summary>
            /// The connection is closed
            /// </summary>
            Closed,

            /// <summary>
            /// The connection is closed, but will open if a handshake is recieved.
            /// </summary>
            Listening,

            /// <summary>
            /// The connection will transmit handshakes to connect to the endpoint.
            /// The connection will be opened if a handshake or acknowledgement is recieved
            /// </summary>
            Opening,

            /// <summary>
            /// This connection is actively communicating to another client
            /// </summary>
            Open
        }

        public ConnectionState State { get; private set; }
        
        private Dictionary<ushort, IncomingSyncPool> IncomingPools = new Dictionary<ushort, IncomingSyncPool>();
        private long timeSinceLastHandshake;
        
        
        public NetworkClient(NetworkConnection connection)
        {
            Connection = connection;
            State = ConnectionState.Closed;
        }

        public void SetState(ConnectionState state)
        {
            switch (state)
            {
                case (ConnectionState.Open):
                case (ConnectionState.Opening):
                    if (State != ConnectionState.Open && State != ConnectionState.Opening)
                    {
                        EnterOpening();
                    }
                    break;

                case (ConnectionState.Listening):
                    if (State != ConnectionState.Listening)
                    {
                        EnterListening();
                    }
                    break;

                case (ConnectionState.Closed):
                    if (State != ConnectionState.Closed)
                    {
                        EnterClosed();
                    }
                    break;
            }
        }

        private void EnterOpen()
        {
            State = ConnectionState.Open;
        }

        private void EnterOpening()
        {
            State = ConnectionState.Opening;
        }

        private void EnterListening()
        {
            if (State == ConnectionState.Open || State == ConnectionState.Opening)
            {
                // Notify any potential endpoint that we are no longer active.
                Connection.Enqueue(new HandshakePayload(State));
            }

            State = ConnectionState.Listening;
        }

        private void EnterClosed()
        {
            if (State == ConnectionState.Open || State == ConnectionState.Opening)
            {
                // Notify any potential endpoint that we are no longer active.
                Connection.Enqueue(new HandshakePayload(State));
            }

            State = ConnectionState.Closed;
        }

        public void Update()
        {
            List<Payload> recieved = Connection.Recieve();
            List<Payload> timeouts = Connection.GetTimeouts();
            
            switch (State)
            {
                case (ConnectionState.Open):
                case (ConnectionState.Opening):
                    foreach (Payload payload in timeouts)
                    {
                        payload.OnTimeout(this);
                    }
                    foreach (Payload payload in recieved)
                    {
                        payload.OnReception(this);
                    }
                    break;
                case (ConnectionState.Listening):
                    foreach (Payload payload in recieved)
                    {
                        if (payload is HandshakePayload)
                        {
                            payload.OnReception(this);
                        }
                    }
                    break;
                case (ConnectionState.Closed):
                    break;
            }

            UpdateConnectionStatus();

            Connection.Transmit();
        }

        internal void RecieveEndpointState(ConnectionState endpointState)
        {
            switch (endpointState)
            {
                case (ConnectionState.Open):
                case (ConnectionState.Opening):
                    if ( State != ConnectionState.Open )
                    {
                        EnterOpen();
                    }
                    break;
                case (ConnectionState.Listening):
                case (ConnectionState.Closed):
                    break;
            }
        }

        internal void Enqueue(Payload payload)
        {
            if (State == ConnectionState.Open)
            {
                Connection.Enqueue(payload);
            }
        }
        
        private void UpdateConnectionStatus()
        {
            if (State == ConnectionState.Opening ||
               (State == ConnectionState.Open && (Connection.Stats.MillisecondsSinceLastAcknowledgement > KeepAliveTimeout)))
            {
                long timestamp = Connection.Timestamp();
                if (timestamp - timeSinceLastHandshake > KeepAliveTimeout)
                {
                    timeSinceLastHandshake = timestamp;
                    Connection.Enqueue(new HandshakePayload(State));
                }
            }

            if (State == ConnectionState.Opening && Connection.Stats.MillisecondsSinceLastAcknowledgement < 20)
            {
                EnterOpen();
            }
        }
  
        internal void AttachSyncPool(IncomingSyncPool syncPool)
        {
            if (IncomingPools.ContainsKey(syncPool.PoolID))
            {
                throw new NetcodeOverloadedException(string.Format("An IncomingSyncPool with PoolID of {0} has already been attached to this NetworkClient", syncPool.PoolID));
            }
            IncomingPools[syncPool.PoolID] = syncPool;
        }

        internal void DetachSyncPool(IncomingSyncPool syncPool)
        {
            IncomingPools.Remove(syncPool.PoolID);
        }

        internal IncomingSyncPool GetSyncPool(ushort poolID)
        {
            if (IncomingPools.ContainsKey(poolID))
            {
                return IncomingPools[poolID];
            }
            return null;
        }
    }
}
