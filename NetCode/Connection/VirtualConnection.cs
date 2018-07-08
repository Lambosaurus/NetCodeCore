using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Connection
{
    public class VirtualConnection : NetworkConnection
    {
        public NetworkSettings Settings { get; private set; }

        private List<VirtualPacket> recievebuffer = new List<VirtualPacket>();
        private VirtualConnection Endpoint = null;
        private Random random = new Random();

        public VirtualConnection()
        {
            Settings = new NetworkSettings();
        }
        
        public void Connect(VirtualConnection endpoint)
        {
            Endpoint = endpoint;
            endpoint.Endpoint = this;
            endpoint.Settings = Settings;
        }

        protected override void SendData(byte[] data)
        {
            if (Settings.Connected && random.NextDouble() > Settings.PacketLoss)
            {
                int delay = ((int)(random.NextDouble() * (Settings.LatencyMax - Settings.LatencyMin)) + Settings.LatencyMin) / 2;
                Endpoint.QueueForRecieve(data, delay);
            }
        }

        private void QueueForRecieve(byte[] data, int delay)
        {
            recievebuffer.Add(
                new VirtualPacket
                {
                    Timestamp = NetTime.Now() + delay,
                    Data = data
                }
            );
        }
        
        protected override List<byte[]> RecieveData()
        {
            long timestamp = NetTime.Now();

            List<byte[]> recieved = new List<byte[]>();
            List<int> removedIndexes = new List<int>();
            for (int i = 0; i < recievebuffer.Count; i++)
            {
                if (recievebuffer[i].Timestamp < timestamp)
                {
                    recieved.Add(recievebuffer[i].Data);
                    recievebuffer.RemoveAt(i--);
                }
            }
            return recieved;
        }

        private struct VirtualPacket
        {
            public long Timestamp;
            public byte[] Data;
        }

        public class NetworkSettings
        {
            /// <summary>
            /// Sets the probability of a packet being lost.
            /// The visible packet loss will be approximately (1 - (1 - PacketLoss)^2)
            /// due to the chance of the acknowledgement message being lost. For small
            /// values of PacketLoss, this is approximately (PacketLoss * 2)
            /// </summary>
            public double PacketLoss = 0.0;
            
            public int LatencyMax = 0;
            public int LatencyMin = 0;

            public bool Connected = true;
        };
}
}
