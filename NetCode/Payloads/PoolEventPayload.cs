using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;
using NetCode.Connection;
using NetCode.SyncEntity;

namespace NetCode.Payloads
{
    public class PoolEventPayload : Payload
    {
        public override PayloadType Type { get { return PayloadType.PoolEvent; } }
        public override bool AcknowledgementRequired { get { return acknowledgementRequired; } }
        public override bool ImmediateTransmitRequired { get { return immediateTransmitRequired; } }


        public ushort PoolID { get; protected set; }
        public long Timestamp { get; protected set; }
        private int EventSize { get; set; }

        private bool acknowledgementRequired;
        private bool immediateTransmitRequired;

        public PoolEventPayload()
        {
        }

        public static PoolEventPayload Generate(ushort poolID, long timestamp, int size, bool ackRequired, bool immediate)
        {
            PoolEventPayload payload = new PoolEventPayload()
            {
                PoolID = poolID,
                Timestamp = timestamp,
                EventSize = size,
                immediateTransmitRequired = immediate,
                acknowledgementRequired = ackRequired,
            };
            payload.AllocateAndWrite();
            return payload;
        }
        
        public override void WriteContent()
        {
            Primitive.WriteUShort(Data, ref DataIndex, PoolID);
            Primitive.WriteLong(Data, ref DataIndex, Timestamp);
            Primitive.WriteByte(Data, ref DataIndex, acknowledgementRequired ? (byte)0x01 : (byte)0x00);
        }

        public override void ReadContent()
        {
            PoolID = Primitive.ReadUShort(Data, ref DataIndex);
            Timestamp = Primitive.ReadLong(Data, ref DataIndex);
            acknowledgementRequired = Primitive.ReadByte(Data, ref DataIndex) > 0;
            EventSize = Size - (HeaderSize + sizeof(ushort) + sizeof(byte) + sizeof(long));
        }

        public void GetEventContentBuffer(out byte[] data, out int index, out int count)
        {
            data = Data;
            index = DataIndex;
            count = EventSize;
        }

        public override int ContentSize()
        {
            return sizeof(ushort) + sizeof(long) + sizeof(byte) + EventSize;
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
                Timestamp -= offset;
                destination.UnpackEventDatagram(this);
            }
        }
    }
}
