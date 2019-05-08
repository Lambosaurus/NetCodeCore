using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;
using NetCode.Connection;

namespace NetCode.Payloads
{
    [EnumeratePayload]
    public class PoolRevisionPayload : Payload
    {
        public override bool AcknowledgementRequired { get { return true; } }
        public override bool ImmediateTransmitRequired { get { return true; } }

        public ushort PoolID { get; private set; }
        public uint Revision { get; private set; }
        public NetBuffer RevisionData { get; private set; }

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
            Buffer.WriteUShort(PoolID);
            Buffer.WriteUInt(Revision);
            RevisionData = Buffer.SubBuffer(RevisionSize);
        }

        public override void ReadContent()
        {
            PoolID = Buffer.ReadUShort();
            Revision = Buffer.ReadUInt();
            RevisionSize = Buffer.Remaining;
            RevisionData = Buffer.SubBuffer(RevisionSize);
        }

        public override int ContentSize()
        {
            return sizeof(ushort) + sizeof(uint) + RevisionSize;
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
