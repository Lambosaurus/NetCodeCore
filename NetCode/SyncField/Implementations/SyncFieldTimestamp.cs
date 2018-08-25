using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField.Implementations
{
    public class SynchronisableTimestamp : SynchronisableLong
    {
        protected override void PostProcess(SyncContext context)
        {
            value -= context.TimestampOffset;
        }
    }
}
