using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.SyncField
{
    public class SyncContext
    {
        public long TimestampOffset { get; private set; }
        public uint Revision { get; private set; }

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
