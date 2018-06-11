using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;
using NetCode.Connection;

namespace NetCode.Packing
{
    public abstract class Payload
    {
        public enum PayloadType { None, Acknowledgement, PoolRevision }
        public abstract PayloadType Type { get; }

        internal byte[] Data;
        internal int Size;
        internal int DataStart;
        internal int DataIndex;

        public const int PAYLOAD_HEADER_SIZE = sizeof(ushort) + sizeof(byte);

        public abstract void WriteContentHeader();
        public abstract void ReadContentHeader();
        public abstract int ContentHeaderSize();

        public abstract bool AcknowledgementRequired { get; }

        public virtual Payload OnTimeout()
        {
            return null;
        }

        public abstract void OnReception(NetworkConnection connection);


        public Payload()
        {
        }
        
        private void WritePayloadHeader()
        {
            Primitive.WriteByte(Data, ref DataIndex, (byte)Type);
            Primitive.WriteUShort(Data, ref DataIndex, (ushort)Size);
        }
        
        private static void ReadPayloadHeader( byte[] data, ref int index, out PayloadType payloadType, out int size )
        {
            payloadType = (PayloadType)Primitive.ReadByte(data, ref index);
            size = Primitive.ReadUShort(data, ref index);
        }
        
        public void AllocateContent(int contentSize)
        {
            Size = contentSize + ContentHeaderSize() + PAYLOAD_HEADER_SIZE;
            Data = new byte[Size];
            DataStart = 0;
            DataIndex = DataStart;

            WritePayloadHeader();
            WriteContentHeader();
        }

        public void AllocateFromExisting(byte[] data, int start, int size)
        {
            Size = size;
            Data = data;
            DataStart = start;
            DataIndex = DataStart + PAYLOAD_HEADER_SIZE; // Skip datagram header.

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
            Buffer.BlockCopy(Data, DataStart, data, index, Size);
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
                case (PayloadType.Acknowledgement):
                    return new AcknowledgementPayload();
                default:
                    return null;
            }
        }
    }
}
