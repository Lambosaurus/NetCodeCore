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

        public SyncHandle GetHandleByEntityID(ushort entityID)
        {
            return Pool.GetHandle(entityID);
        }
    }
}
