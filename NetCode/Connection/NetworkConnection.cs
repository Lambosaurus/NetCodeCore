using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using NetCode.Packing;

namespace NetCode.Connection
{
    public abstract class NetworkConnection
    {
        public ConnectionStatus Status { get; private set; }

        private Stopwatch timer;
        private List<Packet> PendingPackets = new List<Packet>();
        
        public NetworkConnection()
        {
            timer = new Stopwatch();
            timer.Start();
        }

        public void Transmit(Packet packet)
        {
            if (packet.RequiresAcknowledgement())
            {
                PendingPackets.Add(packet);
            }

            Send(packet.Encode());
        }

        protected abstract void Send(byte[] data);
        

        public ConnectionStatus GetConnectionStatus()
        {
            return Status;
        }

        public struct ConnectionStatus
        {
            bool Connected;
            int Latency;
            int WorstRecentLatency;
            double PacketLoss;
            long BytesSent;
            long BytesRecieved;
            int SendRate;
            int RecieveRate;
            int PendingPackets;
        }
    }
}
