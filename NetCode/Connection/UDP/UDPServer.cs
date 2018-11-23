using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;
using System.Net.Sockets;

using NetCode.Payloads;

namespace NetCode.Connection.UDP
{
    public class UDPServer
    {
        public int Port { get; protected set; }
        public int IncomingConnectionLimit { get; set; } = 0;
        public int IncomingConnections { get; private set; } = 0;
        public int OutgoingConnections { get; private set; } = 0;

        private List<UDPFeed> Feeds;
        private UdpClient Socket;
        
        private struct AddressedData
        {
            public byte[] Data;
            public IPEndPoint Source;

            public AddressedData(byte[] data, IPEndPoint source)
            {
                Data = data;
                Source = source;
            }
        }

        private List<AddressedData> UnallocatedIncomingData;
        
        public UDPServer(int port)
        {
            Port = port;
            Feeds = new List<UDPFeed>();
            UnallocatedIncomingData = new List<AddressedData>();
            Socket = new UdpClient(port);
        }
        
        public UDPFeed RecieveConnection()
        {
            if (IncomingConnections >= IncomingConnectionLimit)
            {
                // We cannot allocate any additional incoming connections. Just flush em.
                UnallocatedIncomingData.Clear();
                return null;
            }

            FlushRecieve();

            UDPFeed feed = null;
            List<AddressedData> remainingIncoming = null;

            foreach (AddressedData incoming in UnallocatedIncomingData)
            {

                if (feed == null)
                {
                    // Attempt to decode the packet.
                    if (incoming.Data.Length < (Packet.HeaderSize + Payload.HeaderSize)) { continue; }

                    HandshakePayload req = Packet.Peek<HandshakePayload>(incoming.Data);
                    if (req != null && req.State == NetworkClient.ConnectionState.Opening)
                    {
                        feed = OpenIncomingConnection(incoming.Source);
                        feed.FeedData(incoming.Data); // This data belongs to the new feed.

                        // We will start putting other packets in here for next round.
                        remainingIncoming = new List<AddressedData>();
                        break;
                    }
                }
                else
                {
                    if (!incoming.Source.Equals(feed.Destination))
                    {
                        // The remaining items should be added to the 
                        remainingIncoming.Add(incoming);
                    }
                    else
                    {
                        feed.FeedData(incoming.Data);
                    }
                }
            }

            if (remainingIncoming != null)
            {
                UnallocatedIncomingData = remainingIncoming;
            }
            else
            {
                UnallocatedIncomingData.Clear();
            }

            return feed; // This may be null
        }

        public UDPFeed OpenConnection(IPEndPoint destination)
        {
            UDPFeed feed = new UDPFeed(this, destination, false);
            Feeds.Add(feed);
            OutgoingConnections += 1;
            return feed;
        }

        private UDPFeed OpenIncomingConnection(IPEndPoint destination)
        {
            UDPFeed feed = new UDPFeed(this, destination, true);
            Feeds.Add(feed);
            IncomingConnections += 1;
            return feed;
        }
        
        internal void CloseFeed(UDPFeed feed)
        {
            if (Feeds.Remove(feed))
            {
                if (feed.IsIncoming)
                {
                    IncomingConnections -= 1;
                }
                else
                {
                    OutgoingConnections -= 1;
                }
            }
        }

        internal void FlushRecieve( )
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);

            while (Socket.Available > 0)
            {
                // TODO: The socket was forcibly closed by the host?
                byte[] data = Socket.Receive(ref endpoint);
                bool allocated = false;
                
                foreach (UDPFeed connection in Feeds)
                {
                    if (endpoint.Equals(connection.Destination))
                    {
                        connection.FeedData(data);
                        allocated = true;
                        break;
                    }
                }
                if (!allocated)
                {
                    UnallocatedIncomingData.Add(new AddressedData( data, endpoint ));
                }
            }
        }
        
        internal void Transmit( byte[] data, IPEndPoint endpoint )
        {
            Socket.Send(data, data.Length, endpoint);
        }

        public void Close( )
        {
            foreach (UDPFeed feed in Feeds)
            {
                feed.Destroy();
            }
            Socket.Close();
        }
    }
}
