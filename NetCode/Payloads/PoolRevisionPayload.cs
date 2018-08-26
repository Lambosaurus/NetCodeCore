using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;
using NetCode.Connection;

namespace NetCode.Payloads
{
    public class PoolRevisionPayload : Payload
    {
        public override PayloadType Type { get { return PayloadType.PoolRevision; } }
        public override bool AcknowledgementRequired { get { return true; } }
        public override bool ImmediateTransmitRequired { get { return true; } }

        public ushort PoolID { get; protected set; }
        public uint Revision { get; protected set; }
        
        private OutgoingSyncPool SyncPool;
        private int RevisionSize;


        public PoolRevisionPayload()
        {
        }

        public static PoolRevisionPayload Generate(OutgoingSyncPool syncPool, uint revision, int size)
        {
            PoolRevisionPayload payload = new PoolRevisionPayload()
            {
                PoolID = syncPool.PoolID,
                Revision = revision,
                SyncPool = syncPool,
                RevisionSize = size
            };
            payload.AllocateAndWrite();
            return payload;
        }
        
        public override void WriteContent()
        {
            Primitive.WriteUShort(Data, ref DataIndex, PoolID);
            Primitive.WriteUInt(Data, ref DataIndex, Revision);
        }

        public override void ReadContent()
        {
            PoolID = Primitive.ReadUShort(Data, ref DataIndex);
            Revision = Primitive.ReadUInt(Data, ref DataIndex);
            RevisionSize = Size - (HeaderSize + sizeof(ushort) + sizeof(uint));
        }

        public override int ContentSize()
        {
            return sizeof(ushort) + sizeof(uint) + RevisionSize;
        }

        public void GetRevisionContentBuffer( out byte[] data, out int index, out int count)
        {
            data = Data;
            index = DataIndex;
            count = RevisionSize;
        }

        public override void OnTimeout(NetworkClient client)
        {
            Payload payload = SyncPool.GenerateRevisionPayload(Revision);
            if (payload != null)
            {
                client.Enqueue(payload);
            }
        }

        public override void OnReception(NetworkClient client)
        {
            IncomingSyncPool destination = client.GetSyncPool(PoolID);
            if (destination != null)
            {
                long offset = client.Connection.Stats.NetTimeOffset;
                destination.UnpackRevisionDatagram(this, offset);
            }
        }
    }
}
