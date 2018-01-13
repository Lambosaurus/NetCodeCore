using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class SyncPool
    {
        const int POOLID_HEADER_SIZE = sizeof(ushort);

        public Dictionary<uint, SyncHandle> Handles { get; private set; } = new Dictionary<uint, SyncHandle>();
        public ushort PoolID { get; private set; }

        internal SyncEntityGenerator entityGenerator;

        internal SyncPool(SyncEntityGenerator generator, ushort poolID)
        {
            entityGenerator = generator;
            PoolID = poolID;
        }
        
        protected int HeaderSize()
        {
            return POOLID_HEADER_SIZE;
        }

        internal static void ReadHeader(byte[] data, ref int index, out ushort poolID)
        {
            poolID = PrimitiveSerialiser.ReadUShort(data, ref index);
        }

        protected void WriteHeader(byte[] data, ref int index)
        {
            PrimitiveSerialiser.WriteUShort(data, ref index, PoolID);
        }
    }
}
