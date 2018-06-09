using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using NetCode.Packing;
using NetCode.SyncPool;

namespace NetCode.Connection
{
    public abstract class NetworkConnection
    {
        public ConnectionStats Stats { get; private set; }

        private Stopwatch timer;
        private List<Packet> pendingPackets = new List<Packet>();

        public NetworkConnection()
        {
            Stats = new ConnectionStats();

            timer = new Stopwatch();
            timer.Start();
        }

        private uint lastPacketID = 0;
        private uint GetNewPacketID()
        {
            return ++lastPacketID;
        }
        
        public void Update()
        {
            long timestamp = Timestamp();

            List<byte[]> incomingData = Recieve();
            foreach (byte[] data in incomingData)
            {
                RecievePacket(data);
            }

            Stats.Update(timestamp);
        }

        private long Timestamp()
        {
            return timer.ElapsedMilliseconds;
        }

        public void Transmit(Packet packet)
        {
            if (packet.RequiresAcknowledgement())
            {
                pendingPackets.Add(packet);
            }
            long timestamp = Timestamp();
            byte[] data = packet.Encode(timestamp);
            Stats.RecordSend(data.Length, timestamp);
            Send(data);
        }

        private void RecievePacket(byte[] data)
        {
            long timestamp = Timestamp();
            Packet packet = Packet.Decode(data, timestamp);
            Stats.RecordReceive(data.Length, timestamp, packet.DecodingError);

            foreach( Payload payload in packet.Payloads )
            {
                HandlePayload(payload);
            }
        }

        private void HandlePayload(Payload payload)
        {
            if (payload.Type == Payload.PayloadType.Acknowledgement)
            {
                AcknowledgementPayload ackPayload = (AcknowledgementPayload)payload;
                ackPayload.ReadContent();
                foreach ( uint packetID in ackPayload.PacketIDs )
                {
                    AcknowledgePacket(packetID);
                }
            }
        }
        
        private void AcknowledgePacket(uint packetID)
        {
            Packet packet = pendingPackets.Find(p => p.PacketID == packetID);
            if (packet != null)
            {
                pendingPackets.Remove(packet);
                int latency = (int)(Timestamp() - packet.Timestamp);
                
            }
        }

        protected abstract bool Connected();
        protected abstract void Send(byte[] data);
        protected abstract List<byte[]> Recieve();
    }
}
