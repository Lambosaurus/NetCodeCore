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
    }
}
