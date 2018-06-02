using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public abstract class SynchronisablePool : IVersionable
    {
        const int POOLID_HEADER_SIZE = sizeof(ushort);
        
        public IEnumerable<SyncHandle> Handles  { get {  return SyncHandles.Values; } }
        public ushort PoolID { get; private set; }
        public bool Changed { get; protected set; }

        protected Dictionary<uint, SyncHandle> SyncHandles { get; private set; } = new Dictionary<uint, SyncHandle>();

        internal SyncEntityGenerator entityGenerator;

        internal SynchronisablePool(SyncEntityGenerator generator, ushort poolID)
        {
            entityGenerator = generator;
            PoolID = poolID;
            Changed = false;
        }
        
        protected int HeaderSize()
        {
            return POOLID_HEADER_SIZE;
        }

        public static void ReadHeader(byte[] data, ref int index, out ushort poolID)
        {
            poolID = PrimitiveSerialiser.ReadUShort(data, ref index);
        }

        protected void WriteHeader(byte[] data, ref int index)
        {
            PrimitiveSerialiser.WriteUShort(data, ref index, PoolID);
        }


        public abstract void PushToBuffer(byte[] data, ref int index, uint revision);
        public abstract int PushToBufferSize();
        public abstract void PullFromBuffer(byte[] data, ref int index, uint revisiona);
    }
}
