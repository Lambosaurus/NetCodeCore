using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Payloads
{
    public abstract class Payload
    {
        public enum PayloadType { None, PoolRevision }
        public PayloadType Type;

        internal byte[] Data;
        internal int Size;
        internal int Start;
        internal int Index;

        public Payload(PayloadType type)
        {
            Type = type;
        }
        
        public const int PAYLOAD_HEADER_SIZE = sizeof(ushort) + sizeof(byte);
    
        public void WritePayloadHeader()
        {
            Primitives.WriteByte(Data, ref Index, (byte)Type);
            Primitives.WriteUShort(Data, ref Index, (ushort)Size);
        }
        
        private static void ReadPayloadHeader( byte[] data, ref int index, out PayloadType payloadType, out int size )
        {
            payloadType = (PayloadType)Primitives.ReadByte(data, ref index);
            size = Primitives.ReadUShort(data, ref index);
        }

        public abstract void WriteContentHeader();
        public abstract void ReadContentHeader();
        public abstract int ContentHeaderSize();
        
        public void AllocateContent(int contentSize)
        {
            Size = contentSize + ContentHeaderSize() + PAYLOAD_HEADER_SIZE;
            Data = new byte[Size];
            Start = 0;
            Index = Start;

            WritePayloadHeader();
            WriteContentHeader();
        }

        public void AllocateFromExisting(byte[] data, int start, int size)
        {
            Size = size;
            Data = data;
            Start = start;
            Index = Start + PAYLOAD_HEADER_SIZE; // Skip datagram header.

            ReadContentHeader();
        }

        public static Payload Decode(byte[] data, ref int index)
        {
            int tempIndex = index;
            ReadPayloadHeader(data, ref tempIndex, out PayloadType payloadType, out int size);

            Payload payload = GetPayloadByType(payloadType);

            if (payload == null || index + size > data.Length)
            {
                return null;
            }

            payload.AllocateFromExisting(data, index, size);
            index += size;

            return payload;
        }

        public void CopyContent(byte[] data, ref int index)
        {
            Buffer.BlockCopy(Data, Start, data, index, Size);
            index += Size;
        }

        public void ClearContent()
        {
            Data = null;
        }

        public static Payload GetPayloadByType(PayloadType payloadType)
        {
            switch (payloadType)
            {
                case (PayloadType.PoolRevision):
                    return new PoolRevisionPayload();
                default:
                    return null;
            }
        }
    }
}
