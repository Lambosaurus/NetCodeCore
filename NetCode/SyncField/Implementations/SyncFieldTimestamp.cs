using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.SyncField.Implementations
{
    public class SynchronisableTimestamp : SynchronisableLong
    {
        public override void PostProcess(SyncContext context)
        {
            value -= context.TimestampOffset;
        }
    }
}
