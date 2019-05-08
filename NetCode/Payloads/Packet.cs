using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.Payloads
{
    public class Packet
    {
        internal const int HeaderSize = sizeof(uint);
        
        public bool DecodingError { get; private set; } = false;
        public uint PacketID { get; private set; }
        public long Timestamp { get; private set; }
        public List<Payload> Payloads { get; private set; }
        

        public Packet(uint packetID)
        {
            PacketID = packetID;
            Payloads = new List<Payload>();
        }

        private void WritePacketHeader(NetBuffer buffer)
        {
            buffer.WriteUInt(PacketID);
        }

        private static void ReadPacketHeader(NetBuffer buffer, out uint packetID)
        {
            packetID = buffer.ReadUInt();
        }
        
        public byte[] Encode(long timestamp)
        {
            Timestamp = timestamp;

            int size = HeaderSize;
            foreach (Payload payload in Payloads)
            {
                size += payload.Buffer.Size;
            }

            NetBuffer buffer = new NetBuffer(size);

            WritePacketHeader(buffer);

            foreach (Payload payload in Payloads)
            {
                buffer.WriteBuffer(payload.Buffer);
                //payload.ClearContent();
            }

            return buffer.Data;
        }
        
        public bool RequiresAcknowledgement()
        {
            foreach (Payload payload in Payloads)
            {
                if (payload.AcknowledgementRequired)
                {
                    return true;
                }
            }
            return false;
        }
        
        public static TPayload Peek<TPayload>(byte[] data, int maxDepth = 1) where TPayload : Payload
        {
            NetBuffer buffer = new NetBuffer(data);
            ReadPacketHeader(buffer, out uint packetID);

            while ((buffer.Remaining >= Payload.HeaderSize) && (maxDepth-- > 0))
            {
                TPayload payload = Payload.Peek<TPayload>(buffer);
                if (payload != null)
                {
                    return payload;
                }
            }
            return null;
        }
        
        public static Packet Decode(byte[] data, long timestamp)
        {
            NetBuffer buffer = new NetBuffer(data);

            ReadPacketHeader(buffer, out uint packetID);
            Packet packet = new Packet(packetID);
            packet.Timestamp = timestamp;
            
            while (buffer.Remaining >= Payload.HeaderSize)
            {
                Payload payload = Payload.Decode(buffer);
                if (payload == null)
                {
                    packet.DecodingError = true;
                    break;
                }
                packet.Payloads.Add(payload);
            }

            if (buffer.Remaining != 0)
            {
                packet.DecodingError = true;
            }

            return packet;
        }
    }
}
