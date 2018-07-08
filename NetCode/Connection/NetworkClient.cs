using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

using NetCode.SyncPool;
using NetCode.Payloads;
using NetCode.Util;

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
            None                          = 0     ,
            HandleIncomingPayloads        = 1 << 0,
            HandleIncomingHandshakesOnly  = 1 << 1,
            AllowOutgoingPayloads         = 1 << 2,
            GenerateOutgoingHandshakes    = 1 << 3,
            CloseOnTimeout                = 1 << 4,
        }

        private ConnectionBehavior Behavior;
        private Dictionary<ushort, IncomingSyncPool> IncomingPools = new Dictionary<ushort, IncomingSyncPool>();

        private EventMarker HandshakeSentMarker = new EventMarker();
        private EventMarker KeepAliveMarker = new EventMarker();
        

        public NetworkClient(NetworkConnection connection)
        {
            Connection = connection;
            State = ConnectionState.Closed;
            Behavior = ConnectionBehavior.None;
        }

        /// <summary>
        /// Puts the client into the specified ConnectionState.
        /// </summary>
        /// <param name="requestedState">The state to enter. Open and Opening are equivilent here.</param>
        public void SetState(ConnectionState requestedState)
        {
            switch (requestedState)
            {
                case (ConnectionState.Open):
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
        }

        private void EnterStateOpening()
        {
            State = ConnectionState.Opening;
            Behavior = ConnectionBehavior.GenerateOutgoingHandshakes
                     | ConnectionBehavior.HandleIncomingPayloads
                     | ConnectionBehavior.CloseOnTimeout;


            // Kick the connection off.
            Connection.Enqueue(new HandshakePayload(State));
            HandshakeSentMarker.Mark();
            KeepAliveMarker.Mark();
        }

        private void EnterStateListening()
        {
            State = ConnectionState.Listening;
            if (BehaviorSet(ConnectionBehavior.GenerateOutgoingHandshakes))
            {
                Connection.Enqueue(new HandshakePayload(State));
            }
            Behavior = ConnectionBehavior.HandleIncomingHandshakesOnly;
        }

        private void EnterStateClosed()
        {
            State = ConnectionState.Closed;
            if (BehaviorSet(ConnectionBehavior.GenerateOutgoingHandshakes))
            {
                // If we have just come from a state where handshakes are required, then we should notify the endpoint that we are closing.
                Connection.Enqueue(new HandshakePayload(State));
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
                Connection.DiscardIncomingPackets();
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
                if (   (!KeepAliveMarker.MarkedInPast(KeepAliveTimeout))
                    && (!HandshakeSentMarker.MarkedInPast(KeepAliveTimeout) ))
                {
                    HandshakeSentMarker.Mark();
                    Connection.Enqueue(new HandshakePayload(State));
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


        /// <summary>
        /// This is called by the OnRecieve method of n incoming HandshakePacket.
        /// </summary>
        /// <param name="endpointState">Indicates the state declared by the endpoint client.</param>
        internal void RecieveEndpointState(ConnectionState endpointState)
        {
            switch (endpointState)
            {
                case (ConnectionState.Open):
                case (ConnectionState.Opening):
                    if ( State == ConnectionState.Listening )
                    {
                        EnterStateOpening();
                    }
                    break;
                case (ConnectionState.Listening):
                case (ConnectionState.Closed):
                    if ( State != ConnectionState.Closed )
                    {
                        EnterStateClosed();
                    }
                    break;
            }
        }

        /// <summary>
        /// This will be called by an AcknowledgementPayload
        /// </summary>
        internal void RecieveAcknowledgement()
        {
            KeepAliveMarker.Mark();

            if ( State == ConnectionState.Opening )
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
