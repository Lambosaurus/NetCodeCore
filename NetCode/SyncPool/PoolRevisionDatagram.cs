using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Packet;

namespace NetCode.SyncPool
{
    public class PoolRevisionDatagram : Datagram
    {
        public ushort PoolID { get; protected set; }
        public uint Revision { get; protected set; }

        public PoolRevisionDatagram() : base(Datatype.PoolRevision)
        {
        }

        public PoolRevisionDatagram(ushort poolID, uint revision) : base(Datatype.PoolRevision)
        {
            PoolID = poolID;
            Revision = revision;
        }
        
        public override void WriteContentHeader()
        {
            Primitives.WriteUShort(Data, ref Index, PoolID);
            Primitives.WriteUInt(Data, ref Index, Revision);
        }

        public override void ReadContentHeader()
        {
            PoolID = Primitives.ReadUShort(Data, ref Index);
            Revision = Primitives.ReadUInt(Data, ref Index);
        }

        public override int ContentHeaderSize()
        {
            return sizeof(ushort) + sizeof(uint);
        }
        
    }
}
