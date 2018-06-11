﻿using System;
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
        public uint PacketTimeout { get; set; } = 2000;

        private Stopwatch timer;
        private List<Packet> pendingPackets = new List<Packet>();
        private List<Payload> payloadQueue = new List<Payload>();

        private Dictionary<ushort, IncomingSyncPool> recievingPools = new Dictionary<ushort, IncomingSyncPool>();

        private List<uint> packetAcknowledgementQueue = new List<uint>();
        
        public NetworkConnection()
        {
            Stats = new ConnectionStats();

            timer = new Stopwatch();
            timer.Start();
        }

        internal void AttachSyncPool(IncomingSyncPool syncPool)
        {
            if (recievingPools.ContainsKey(syncPool.PoolID))
            {
                throw new NetcodeOverloadedException(string.Format("An IncomingSyncPool with PoolID of {0} has already been attached to this NetworkConnection", syncPool.PoolID));
            }
            recievingPools[syncPool.PoolID] = syncPool;
        }

        internal void DetachSyncPool(IncomingSyncPool syncPool)
        {
            recievingPools.Remove(syncPool.PoolID);
        }
        
        internal IncomingSyncPool GetSyncPool(ushort poolID)
        {
            if (recievingPools.ContainsKey(poolID))
            {
                return recievingPools[poolID];
            }
            return null;
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

            UpdatePacketTimeouts(timestamp);

            GenerateAcknowledgementPayloads();

            while (payloadQueue.Count > 0)
            {
                Packet packet = ConstructPacket();
                Transmit(packet);
            }

            Stats.Update(timestamp);
        }

        private long Timestamp()
        {
            return timer.ElapsedMilliseconds;
        }

        public void Enqueue(Payload payload)
        {
            payloadQueue.Add(payload);
        }

        public void EnqueueAcknowledgement(uint packetID)
        {
            packetAcknowledgementQueue.Add(packetID);
        }

        private void GenerateAcknowledgementPayloads()
        {
            if ( packetAcknowledgementQueue.Count > 0 )
            {
                Enqueue(new AcknowledgementPayload(packetAcknowledgementQueue));
                packetAcknowledgementQueue.Clear();
            }
        }

        private Packet ConstructPacket()
        {
            uint packetID = GetNewPacketID();
            Packet packet = new Packet(packetID);

            packet.Payloads.AddRange(payloadQueue);
            payloadQueue.Clear();

            return packet;
        }
        
        private void Transmit(Packet packet)
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
                payload.OnReception(this);
            }

            if (packet.RequiresAcknowledgement())
            {
                EnqueueAcknowledgement(packet.PacketID);
            }
        }
        
        internal void AcknowledgePacket(uint packetID)
        {
            Packet packet = pendingPackets.Find(p => p.PacketID == packetID);
            if (packet != null)
            {
                pendingPackets.Remove(packet);
                long timestamp = Timestamp();
                int latency = (int)(timestamp - packet.Timestamp);
                Stats.RecordAcknowledgement(latency, timestamp);
            }
        }

        private void UpdatePacketTimeouts(long timestamp)
        {
            // Assuming packets are ordered by timestamp.
            int culledPackets = 0;
            foreach( Packet packet in pendingPackets )
            {
                if ( timestamp - packet.Timestamp < PacketTimeout )
                {
                    culledPackets++;
                    Stats.RecordTimeout(timestamp);
                    foreach(Payload payload in packet.Payloads)
                    {
                        payload.OnTimeout();
                    }
                }
                else
                {
                    break;
                }
            }
            pendingPackets.RemoveRange(0, culledPackets);
        }

        protected abstract bool Connected();
        protected abstract void Send(byte[] data);
        protected abstract List<byte[]> Recieve();
    }
}
