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
        Stopwatch timer;
        
        public NetworkConnection()
        {
            timer = new Stopwatch();
            timer.Start();
        }

        public void Transmit(Packet packet)
        {
        }

        protected abstract void Send(byte[] data);
        
        public class ConnectionStatus
        {
            public bool Connected;
            public int Latency;
            public float PacketLoss;
        }
    }
}
