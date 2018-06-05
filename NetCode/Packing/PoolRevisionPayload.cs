using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.Packing
{
    public class PoolRevisionPayload : Payload
    {
        public ushort PoolID { get; protected set; }
        public uint Revision { get; protected set; }

        public PoolRevisionPayload() : base(PayloadType.PoolRevision)
        {
        }

        public PoolRevisionPayload(ushort poolID, uint revision) : base(PayloadType.PoolRevision)
        {
            PoolID = poolID;
            Revision = revision;
        }
        
        public override void WriteContentHeader()
        {
            Primitive.WriteUShort(Data, ref Index, PoolID);
            Primitive.WriteUInt(Data, ref Index, Revision);
        }

        public override void ReadContentHeader()
        {
            PoolID = Primitive.ReadUShort(Data, ref Index);
            Revision = Primitive.ReadUInt(Data, ref Index);
        }

        public override int ContentHeaderSize()
        {
            return sizeof(ushort) + sizeof(uint);
        }

        public override bool AcknowledgementRequired()
        {
            return true;
        }

    }
}
