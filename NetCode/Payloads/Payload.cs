using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.Connection;

// Used only for payload definitions
using NetCode.Connection.UDP;


namespace NetCode.Payloads
{
    public abstract class Payload
    {
        public enum PayloadType { None, Handshake, Acknowledgement, PoolRevision, PoolDeletion, UDPConnectionRequest }
        public abstract PayloadType Type { get; }

        public int Size { get; protected set; }

        protected byte[] Data;
        protected int DataStart;
        protected int DataIndex;

        internal const int HeaderSize = sizeof(ushort) + sizeof(byte);

        public abstract void WriteContent();
        public abstract void ReadContent();
        public abstract int ContentSize();

        /// <summary>
        /// Indicates whether the packet must be acknowledged by the endpoint.
        /// OnTimout will be called if this is true and no acknowledgement is recieved.
        /// </summary>
        public abstract bool AcknowledgementRequired { get; }

        /// <summary>
        /// Indicates whether this payload may be delayed so that it may be packed
        /// with other payloads. This will reduce traffic, at the cost of a few hundred ms.
        /// </summary>
        public abstract bool ImmediateTransmitRequired { get; }


        /// <summary>
        /// The payload may timeout for each connection it is enqueued into.
        /// </summary>
        /// <param name="connection"></param>
        public virtual void OnTimeout(NetworkClient connection)
        {
        }
        
        public abstract void OnReception(NetworkClient connection);


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
        
        public void AllocateAndWrite()
        {
            Size = ContentSize() + HeaderSize;
            Data = new byte[Size];
            DataStart = 0;
            DataIndex = DataStart;

            WritePayloadHeader();
            WriteContent();
        }

        public void ReadFromExisting(byte[] data, int start, int size)
        {
            Size = size;
            Data = data;
            DataStart = start;
            DataIndex = DataStart + HeaderSize; // Skip datagram header.

            ReadContent();
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

            payload.ReadFromExisting(data, index, size);
            index += size;

            return payload;
        }

        public void CopyContent(byte[] data, ref int index)
        {
            Buffer.BlockCopy(Data, DataStart, data, index, Size);
            index += Size;
        }
        
        public static Payload GetPayloadByType(PayloadType payloadType)
        {
            switch (payloadType)
            {
                case (PayloadType.Handshake):
                    return new HandshakePayload();
                case (PayloadType.Acknowledgement):
                    return new AcknowledgementPayload();
                case (PayloadType.PoolRevision):
                    return new PoolRevisionPayload();
                case (PayloadType.PoolDeletion):
                    return new PoolDeletionPayload();
                case (PayloadType.UDPConnectionRequest):
                    return new UDPConnectionRequestPayload();
                default:
                    return null;
            }
        }
    }
}
