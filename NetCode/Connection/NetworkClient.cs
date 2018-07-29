using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

using NetCode.SyncPool;
using NetCode.Payloads;
using NetCode.Util;
using NetCode.Util.Aggregators;

namespace NetCode.Connection
{
    public class NetworkClient
    {
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
        public int KeepAliveTimeout { get; set; } = 300;
        public int ConnectionClosedTimeout { get; set; } = 5000;
        public NetworkConnection Connection { get; private set; }
        

        [Flags]
        private enum ConnectionBehavior {
            None                                = 0     ,
            HandleIncomingPayloads              = 1 << 0,
            HandleIncomingHandshakesOnly        = 1 << 1,
            AllowOutgoingPayloads               = 1 << 2,
            GenerateOutgoingHandshakes          = 1 << 3,
            GenerateOutgoingConnectionRequests  = 1 << 4,
            CloseOnTimeout                      = 1 << 5,
        }

        private ConnectionBehavior Behavior;
        private Dictionary<ushort, IncomingSyncPool> IncomingPools = new Dictionary<ushort, IncomingSyncPool>();
        private Dictionary<ushort, OutgoingSyncPool> OutgoingPools = new Dictionary<ushort, OutgoingSyncPool>();

        private EventMarker HandshakeSentMarker = new EventMarker();
        private EventMarker KeepAliveMarker = new EventMarker();
        

        public NetworkClient(NetworkConnection connection)
        {
            Connection = connection;
            State = ConnectionState.Closed;
            Behavior = ConnectionBehavior.None;
        }

        public void SetState(ConnectionState requestedState)
        {
            switch (requestedState)
            {
                case (ConnectionState.Open):
                    // We must always transition to Open through the Opening state.
                    // This ensure the connection is set up and that acknowledgmets comes through.
                case (ConnectionState.Opening):
                    if (State != ConnectionState.Open && State != ConnectionState.Opening)
                    {
                        EnterStateOpening();
                    }
                    break;

                case (ConnectionState.Listening):
                    if (State != ConnectionState.Listening)
                    {
                        EnterStateListening();
                    }
                    break;

                case (ConnectionState.Closed):
                    if (State != ConnectionState.Closed)
                    {
                        EnterStateClosed();
                    }
                    break;
            }
        }

        /// <summary>
        /// This is called by the OnRecieve method of an incoming HandshakePacket.
        /// </summary>
        /// <param name="endpointState">Indicates the state declared by the endpoint client.</param>
        internal void RecieveEndpointState(ConnectionState endpointState)
        {
            switch (endpointState)
            {
                case (ConnectionState.Open):
                case (ConnectionState.Opening):
                    if (State == ConnectionState.Listening)
                    {
                        // Do not transition directly to Open.
                        EnterStateOpening();
                    }
                    break;
                case (ConnectionState.Listening):
                case (ConnectionState.Closed):
                    if (State != ConnectionState.Closed)
                    {
                        EnterStateClosed();
                    }
                    break;
            }
        }

        /// <summary>
        /// This is called by the OnRecieve method of an incoming AcknowledgementPayload.
        /// </summary>
        internal void RecieveAcknowledgement()
        {
            KeepAliveMarker.Mark();

            if (State == ConnectionState.Opening)
            {
                EnterStateOpen();
            }
        }

        internal void Enqueue(Payload payload)
        {
            if (BehaviorSet(ConnectionBehavior.AllowOutgoingPayloads))
            {
                Connection.Enqueue(payload);
            }
        }

        public void Attach(IncomingSyncPool syncPool)
        {
            if (IncomingPools.ContainsKey(syncPool.PoolID))
            {
                throw new NetcodeOverloadedException(string.Format("An IncomingSyncPool with PoolID of {0} has already been attached to this NetworkClient", syncPool.PoolID));
            }
            IncomingPools[syncPool.PoolID] = syncPool;
        }

        public void Detach(IncomingSyncPool syncPool)
        {
            IncomingPools.Remove(syncPool.PoolID);
        }

        public void Attach(OutgoingSyncPool syncPool)
        {
            if (OutgoingPools.ContainsKey(syncPool.PoolID))
            {
                throw new NetcodeOverloadedException(string.Format("An OutgoingSyncPool with PoolID of {0} has already been attached to this NetworkClient", syncPool.PoolID));
            }
            syncPool.Subscribe(this);
            OutgoingPools[syncPool.PoolID] = syncPool;
        }

        public void Detach(OutgoingSyncPool syncPool)
        {
            syncPool.Unsubscribe(this);
            OutgoingPools.Remove(syncPool.PoolID);
        }

        internal IncomingSyncPool GetSyncPool(ushort poolID)
        {
            if (IncomingPools.ContainsKey(poolID))
            {
                return IncomingPools[poolID];
            }
            return null;
        }
        

        [MethodImpl(256)] // Set for aggressive inlining.
        private bool BehaviorSet( ConnectionBehavior behavior )
        {
            return (Behavior & behavior) != 0;
        }

        private void EnterStateOpen()
        {
            State = ConnectionState.Open;
            Behavior = ConnectionBehavior.AllowOutgoingPayloads
                     | ConnectionBehavior.HandleIncomingPayloads
                     | ConnectionBehavior.GenerateOutgoingHandshakes
                     | ConnectionBehavior.CloseOnTimeout;

            EnqueueSetupPayloads();
        }

        private void EnterStateOpening()
        {
            State = ConnectionState.Opening;
            Behavior = ConnectionBehavior.GenerateOutgoingHandshakes
                     | ConnectionBehavior.HandleIncomingPayloads
                     | ConnectionBehavior.CloseOnTimeout
                     | ConnectionBehavior.GenerateOutgoingConnectionRequests;

            // Kick the connection off.
            EnqueueHandshake(true);
            KeepAliveMarker.Mark();
            ClearIncomingSyncPools();
        }

        private void EnterStateListening()
        {
            State = ConnectionState.Listening;
            if (BehaviorSet(ConnectionBehavior.GenerateOutgoingHandshakes))
            {
                Connection.Enqueue(HandshakePayload.Generate(State, false));
            }
            Behavior = ConnectionBehavior.HandleIncomingHandshakesOnly;
        }

        private void EnterStateClosed()
        {
            State = ConnectionState.Closed;
            if (BehaviorSet(ConnectionBehavior.GenerateOutgoingHandshakes))
            {
                // If we have just come from a state where handshakes are required, then we should notify the endpoint that we are closing.
                Connection.Enqueue(HandshakePayload.Generate(State, false));
            }
            Behavior = ConnectionBehavior.None;
        }
        
        public void Update()
        {
            if (BehaviorSet(ConnectionBehavior.HandleIncomingPayloads))
            {
                List<Payload> recieved = Connection.RecievePackets();
                foreach (Payload payload in recieved)
                {
                    payload.OnReception(this);
                }
            }
            else if (BehaviorSet(ConnectionBehavior.HandleIncomingHandshakesOnly))
            {
                List<Payload> recieved = Connection.RecievePackets();
                foreach (Payload payload in recieved)
                {
                    if (payload is HandshakePayload)
                    {
                        payload.OnReception(this);
                    }
                }
            }
            else
            {
                Connection.FlushRecievedPackets();
            }

            // This is done after all payload.OnReception() calls because they may contain relevant AcknowledgementPayloads
            List<Payload> timeouts = Connection.GetTimeouts();
            if (BehaviorSet(ConnectionBehavior.HandleIncomingPayloads))
            {
                foreach (Payload payload in timeouts)
                {
                    payload.OnTimeout(this);
                }
            }
            
            if (BehaviorSet(ConnectionBehavior.GenerateOutgoingHandshakes))
            {
                // periodically generate handshakes unless acknowledgements are being marked.
                if (!HandshakeSentMarker.MarkedInPast(KeepAliveTimeout))
                {
                    bool ackRequired = !KeepAliveMarker.MarkedInPast(KeepAliveTimeout);
                    EnqueueHandshake(ackRequired);
                }
            }

            if (BehaviorSet(ConnectionBehavior.CloseOnTimeout))
            {
                if ( !KeepAliveMarker.MarkedInPast(ConnectionClosedTimeout) )
                {
                    EnterStateClosed();
                }
            }

            Connection.TransmitPackets();
        }

        private void EnqueueHandshake(bool ackRequired)
        {
            HandshakeSentMarker.Mark();
            Connection.Enqueue(HandshakePayload.Generate(State, ackRequired));
            if (BehaviorSet(ConnectionBehavior.GenerateOutgoingConnectionRequests))
            {
                Payload request = Connection.GetConnectionRequestPayload();
                if (request != null)
                {
                    Connection.Enqueue(request);
                }
            }
        }
        
        private void EnqueueSetupPayloads()
        {
            foreach (OutgoingSyncPool pool in OutgoingPools.Values)
            {
                Enqueue(pool.GenerateCompleteStatePayload());
            }
        }

        private void ClearIncomingSyncPools()
        {
            foreach (IncomingSyncPool pool in IncomingPools.Values)
            {
                pool.Clear();
            }
        }
    }
}
