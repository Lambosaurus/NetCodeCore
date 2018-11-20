using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NetCode.SyncPool
{
    public class SyncContext
    {
        public long TimestampOffset { get; set; }
        public uint Revision { get; set; }

        private SynchronisablePool Pool;

        public SyncContext( SynchronisablePool pool, uint revision, long timestampOffset )
        {
            Pool = pool;
            Revision = revision;
            TimestampOffset = timestampOffset;
        }

        public SyncHandle GetHandleByObject(object obj)
        {
            return Pool.GetHandleByObject(obj);
        }

        public SyncHandle GetLinkedHandleByObject(object obj, out ushort poolID)
        {
            SyncHandle handle;

            // Try the resources first
            foreach (SynchronisablePool resource in Pool.ResourceSyncPools)
            {
                handle = resource.GetHandleByObject(obj);
                if (handle != null)
                {
                    poolID = resource.PoolID;
                    return handle;
                }
            }

            // Fallback to the current pool
            handle = Pool.GetHandleByObject(obj);
            poolID = (handle != null) ? Pool.PoolID : (ushort)0;
            return handle;
        }

        public SyncHandle GetHandle(ushort entityID)
        {
            return Pool.GetHandle(entityID);
        }

        public SyncHandle GetHandle(ushort entityID, ushort poolID)
        {
            if (poolID == Pool.PoolID) { return Pool.GetHandle(entityID); }
            foreach ( SynchronisablePool resource in Pool.ResourceSyncPools )
            {
                if (resource.PoolID == poolID)
                {
                    return resource.GetHandle(entityID);
                }
            }
            return null;
        }
    }
}
