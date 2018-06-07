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

        public override PayloadType Type { get { return PayloadType.PoolRevision; } }

        public PoolRevisionPayload()
        {
        }

        public PoolRevisionPayload(ushort poolID, uint revision)
        {
            PoolID = poolID;
            Revision = revision;
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

        public override bool AcknowledgementRequired()
        {
            return true;
        }

    }
}
