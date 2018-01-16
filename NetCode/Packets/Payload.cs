using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Packets
{
    public class Payload
    {
        public enum PayloadType { None, PoolUpdate };

        public PayloadType Type { get; private set; }
        public uint PacketID { get; private set; }

        public byte[] Data = null;

        const int TYPE_PACKET_HEADER = sizeof(byte);
        const int SIZE_PACKET_HEADER = sizeof(ushort);

        public Payload(PayloadType type, uint packetID)
        {
            Type = type;
            PacketID = packetID;
        }
        
        public int WriteSize()
        {
            return Data.Length + TYPE_PACKET_HEADER + SIZE_PACKET_HEADER;
        }


        public void WriteToPacket(byte[] data, ref int index)
        {
            PrimitiveSerialiser.WriteByte(data, ref index, (byte)Type);
            PrimitiveSerialiser.WriteByte(data, ref index, (byte)Type);

            System.Buffer.BlockCopy(Data, 0, data, index, Data.Length);
            index += Data.Length;
        }

        public static Payload ReadFromPacket(byte[] data, ref int index, uint packetID)
        {
            PayloadType type = (PayloadType)PrimitiveSerialiser.ReadByte(data, ref index);
            ushort size = PrimitiveSerialiser.ReadUShort(data, ref index);

            Payload payload = new Payload(type, packetID);

            payload.Data = new byte[size];
            System.Buffer.BlockCopy(data, index, payload.Data, 0, size);
            index += size;

            return payload;
        }
    }
}
