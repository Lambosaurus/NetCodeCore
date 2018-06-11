using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;

namespace NetCode.Packing
{
    public class Packet
    {
        public List<Payload> Payloads { get; private set; }

        public bool DecodingError { get; private set; } = false;
        public uint PacketID { get; private set; }

        const int PACKET_HEADER_SIZE = sizeof(uint);

        public long Timestamp { get; private set; }

        public Packet(uint packetID)
        {
            PacketID = packetID;
            Payloads = new List<Payload>();
        }

        private void WritePacketHeader(byte[] data, ref int index)
        {
            Primitive.WriteUInt(data, ref index, PacketID);
        }

        private static void ReadPacketHeader(byte[] data, ref int index, out uint packetID)
        {
            packetID = Primitive.ReadUInt(data, ref index);
        }
        
        public byte[] Encode(long timestamp)
        {
            Timestamp = timestamp;

            int size = PACKET_HEADER_SIZE;
            foreach (Payload payload in Payloads)
            {
                size += payload.Size;
            }

            int index = 0;
            byte[] data = new byte[size];

            WritePacketHeader(data, ref index);

            foreach (Payload payload in Payloads)
            {
                payload.CopyContent(data, ref index);
                payload.ClearContent();
            }

            return data;
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
        
        
        public static Packet Decode(byte[] data, long timestamp)
        {
            int index = 0;
            int length = data.Length;

            ReadPacketHeader(data, ref index, out uint packetID);
            Packet packet = new Packet(packetID);
            packet.Timestamp = timestamp;
            
            while (index + Payload.PAYLOAD_HEADER_SIZE < length)
            {
                Payload payload = Payload.Decode(data, ref index);
                if (payload == null)
                {
                    break;
                }
                packet.Payloads.Add(payload);
            }

            if (index != length)
            {
                packet.DecodingError = true;
            }

            return packet;
        }
    }
}
