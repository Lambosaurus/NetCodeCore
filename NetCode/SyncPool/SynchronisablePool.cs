using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public abstract class SynchronisablePool
    {
        public IEnumerable<SyncHandle> Handles  { get {  return SyncHandles.Values; } }
        public ushort PoolID { get; private set; }
        public uint Revision { get; protected set; }

        protected Dictionary<uint, SyncHandle> SyncHandles { get; private set; } = new Dictionary<uint, SyncHandle>();

        internal SyncEntityGenerator entityGenerator;

        internal SynchronisablePool(SyncEntityGenerator generator, ushort poolID)
        {
            entityGenerator = generator;
            PoolID = poolID;
        }
    }
}
