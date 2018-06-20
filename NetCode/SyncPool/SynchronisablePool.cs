using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public abstract class SynchronisablePool
    {
        public IEnumerable<SyncHandle> Handles  { get {  return SyncHandles.Values; } }
        public ushort PoolID { get; private set; }
        public uint Revision { get; protected set; }

        protected Dictionary<ushort, SyncHandle> SyncHandles { get; private set; } = new Dictionary<ushort, SyncHandle>();

        internal SyncEntityGenerator entityGenerator;

        internal SynchronisablePool(SyncEntityGenerator generator, ushort poolID)
        {
            entityGenerator = generator;
            PoolID = poolID;
        }

        public SyncHandle GetHandleByObject(object obj)
        {
            foreach ( SyncHandle handle in SyncHandles.Values )
            {
                if (handle.Obj == obj)
                {
                    return handle;
                }
            }
            return null;
        }

        public SyncHandle GetHandle(ushort entityID)
        {
            if (SyncHandles.ContainsKey(entityID))
            {
                return SyncHandles[entityID];
            }
            return null;
        }

        public bool HandleExists(ushort entityID)
        {
            return SyncHandles.ContainsKey(entityID);
        }
    }
}
