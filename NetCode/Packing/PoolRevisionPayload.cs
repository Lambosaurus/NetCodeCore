using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;
using NetCode.SyncPool;
using NetCode.Connection;

namespace NetCode.Packing
{
    public class PoolRevisionPayload : Payload
    {
        public ushort PoolID { get; protected set; }
        public uint Revision { get; protected set; }

        public override PayloadType Type { get { return PayloadType.PoolRevision; } }
        public override bool AcknowledgementRequired { get { return true; } }

        private OutgoingSyncPool SyncPool;

        public PoolRevisionPayload()
        {
        }

        public PoolRevisionPayload(OutgoingSyncPool syncPool, uint revision)
        {
            PoolID = syncPool.PoolID;
            Revision = revision;
            SyncPool = syncPool;
        }
        
        public override void WriteContentHeader()
        {
            Primitive.WriteUShort(Data, ref DataIndex, PoolID);
            Primitive.WriteUInt(Data, ref DataIndex, Revision);
        }

        public override void ReadContentHeader()
        {
            PoolID = Primitive.ReadUShort(Data, ref DataIndex);
            Revision = Primitive.ReadUInt(Data, ref DataIndex);
        }

        public override int ContentHeaderSize()
        {
            return sizeof(ushort) + sizeof(uint);
        }

        public override void OnTimeout(NetworkConnection connection)
        {
            Payload payload = SyncPool.GenerateRevisionPayload(Revision);
            if (payload != null)
            {
                connection.Enqueue(payload);
            }
        }

        public override void OnReception(NetworkConnection connection)
        {
            IncomingSyncPool destination = connection.GetSyncPool(PoolID);
            destination.UnpackRevisionDatagram(this);
        }
    }
}
