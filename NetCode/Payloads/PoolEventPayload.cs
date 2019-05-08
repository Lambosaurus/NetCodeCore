using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;
using NetCode.Connection;
using NetCode.SyncEntity;

namespace NetCode.Payloads
{
    [EnumeratePayload]
    public class PoolEventPayload : Payload
    {
        public override bool AcknowledgementRequired { get { return acknowledgementRequired; } }
        public override bool ImmediateTransmitRequired { get { return immediateTransmitRequired; } }


        public ushort PoolID { get; private set; }
        private int EventSize { get; set; }

        public NetBuffer EventData { get; private set; }

        private bool acknowledgementRequired;
        private bool immediateTransmitRequired;

        public PoolEventPayload()
        {
        }

        public static PoolEventPayload Generate(ushort poolID, int size, bool ackRequired, bool immediate)
        {
            PoolEventPayload payload = new PoolEventPayload()
            {
                PoolID = poolID,
                EventSize = size,
                immediateTransmitRequired = immediate,
                acknowledgementRequired = ackRequired,
            };
            payload.AllocateAndWrite();
            return payload;
        }
        
        public override void WriteContent()
        {
            Buffer.WriteUShort(PoolID);
            Buffer.WriteByte(acknowledgementRequired ? (byte)0x01 : (byte)0x00);
            EventData = Buffer.SubBuffer(EventSize);
        }

        public override void ReadContent()
        {
            PoolID = Buffer.ReadUShort();
            acknowledgementRequired = Buffer.ReadByte() > 0;
            EventSize = Buffer.Remaining;
            EventData = Buffer.SubBuffer(EventSize);
        }

        public override int ContentSize()
        {
            return sizeof(ushort) + sizeof(byte) + EventSize;
        }

        public override void OnTimeout(NetworkClient client)
        {
            client.Enqueue(this);
        }

        public override void OnReception(NetworkClient client)
        {
            IncomingSyncPool destination = client.GetSyncPool(PoolID);
            if (destination != null)
            {
                long offset = client.Connection.Stats.NetTimeOffset;
                destination.UnpackEventDatagram(this);
            }
        }
    }
}
