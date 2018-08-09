using System;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using NetCode.Payloads;
using NetCode.Util;

namespace NetCode.Connection
{
    public abstract class NetworkConnection
    {
        public ConnectionStats Stats { get; private set; }
        public int PacketTimeout { get; set; } = 1000;

        private List<Packet> pendingPackets = new List<Packet>();
        private List<Payload> payloadQueue = new List<Payload>();
        
        private List<uint> packetAcknowledgementQueue = new List<uint>();
        

        public NetworkConnection()
        {
            Stats = new ConnectionStats();
        }
        
        internal List<Payload> RecievePackets()
        {
            List<Payload> payloads = new List<Payload>();
            
            List<byte[]> incomingData = RecieveData();
            if (incomingData != null)
            {
                foreach (byte[] data in incomingData)
                {
                    payloads.AddRange(RecievePacket(data));
                }
            }
            return payloads;
        }

        internal void FlushRecievedPackets()
        {
            RecieveData();
        }

        internal void TransmitPackets()
        {
            long timestamp = NetTime.Now();

            FlushAcknowledgements();
            
            while (payloadQueue.Count > 0)
            {
                Packet packet = ConstructPacket();
                SendPacket(packet);
            }

            Stats.Update(timestamp);
        }

        internal void Enqueue(Payload payload)
        {
            payloadQueue.Add(payload);
        }
        
        internal bool AcknowledgePacket(uint packetID)
        {
            Packet packet = pendingPackets.Find(p => p.PacketID == packetID);
            if (packet != null)
            {
                pendingPackets.Remove(packet);
                long timestamp = NetTime.Now();
                int latency = (int)(timestamp - packet.Timestamp);
                Stats.RecordAcknowledgement(latency, timestamp);
                return true;
            }
            return false;
        }

        internal List<Payload> GetTimeouts()
        {
            long timestamp = NetTime.Now();
            // Assuming packets are ordered by timestamp.
            List<Payload> timeouts = new List<Payload>();
            int culledPackets = 0;
            foreach (Packet packet in pendingPackets)
            {
                if (timestamp - packet.Timestamp > PacketTimeout)
                {
                    culledPackets++;
                    Stats.RecordTimeout(timestamp);
                    timeouts.AddRange(packet.Payloads);
                }
                else
                {
                    break;
                }
            }
            pendingPackets.RemoveRange(0, culledPackets);
            return timeouts;
        }

        private uint lastPacketID = 0;
        private uint GetNewPacketID()
        {
            return ++lastPacketID;
        }

        private Packet ConstructPacket()
        {
            uint packetID = GetNewPacketID();
            Packet packet = new Packet(packetID);

            packet.Payloads.AddRange(payloadQueue);
            payloadQueue.Clear();

            return packet;
        }
        
        private void SendPacket(Packet packet)
        {
            if (packet.RequiresAcknowledgement())
            {
                pendingPackets.Add(packet);
            }
            long timestamp = NetTime.Now();
            byte[] data = packet.Encode(timestamp);
            Stats.RecordSend(data.Length, timestamp);
            SendData(data);
        }

        private List<Payload> RecievePacket(byte[] data)
        {
            long timestamp = NetTime.Now();
            Packet packet = Packet.Decode(data, timestamp);
            Stats.RecordReceive(data.Length, timestamp, packet.DecodingError);
            
            if (packet.RequiresAcknowledgement())
            {
                packetAcknowledgementQueue.Add(packet.PacketID);
            }

            return packet.Payloads;
        }

        private void FlushAcknowledgements()
        {
            if (packetAcknowledgementQueue.Count > 0)
            {
                foreach (uint[] packetIDs in packetAcknowledgementQueue.Segment(PoolDeletionPayload.MAX_ENTITY_IDS))
                {
                    Enqueue(AcknowledgementPayload.Generate(packetIDs));
                }
                packetAcknowledgementQueue.Clear();
            }
        }

        
        internal virtual Payload GetConnectionRequestPayload() { return null; }

        public abstract void Destroy();
        protected abstract void SendData(byte[] data);
        protected abstract List<byte[]> RecieveData();
    }
}
