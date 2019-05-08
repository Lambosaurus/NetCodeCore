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
        public NetBuffer Buffer { get; private set; }

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
            Buffer.WriteByte(PayloadGenerator.GetPayloadID(this.GetType().TypeHandle));
            Buffer.WriteUShort((ushort)Buffer.Size);
        }
        
        private static void ReadPayloadHeader( NetBuffer buffer, out byte payloadType, out int size )
        {
            payloadType = buffer.ReadByte();
            size = buffer.ReadUShort();
        }
        
        public void AllocateAndWrite()
        {
            int size = ContentSize() + HeaderSize;
            Buffer = new NetBuffer(size);
            WritePayloadHeader();
            WriteContent();
        }

        public void ReadFromExisting(NetBuffer buffer)
        {
            Buffer = buffer;
            Buffer.Index += HeaderSize;
            ReadContent();
        }

        public static Payload Decode(NetBuffer buffer)
        {
            ReadPayloadHeader(buffer, out byte payloadType, out int size);
            buffer.Index -= HeaderSize;
            Payload payload = PayloadGenerator.GeneratePayload(payloadType);
            if (payload == null || buffer.Remaining < size) { return null; }
            payload.ReadFromExisting( buffer.SubBuffer(size) );
            return payload;
        }

        public static TPayload Peek<TPayload>(NetBuffer buffer) where TPayload : Payload
        {
            ReadPayloadHeader(buffer, out byte payloadType, out int size);
            buffer.Index -= HeaderSize;
            if (payloadType == PayloadGenerator.GetPayloadID(typeof(TPayload).TypeHandle))
            {
                Payload payload = PayloadGenerator.GeneratePayload(payloadType);
                if (payload == null || buffer.Remaining < size) { return null; }
                payload.ReadFromExisting(buffer.SubBuffer(size));
                return (TPayload)payload;
            }
            buffer.Index += size;
            return null;
        }
    }
}
