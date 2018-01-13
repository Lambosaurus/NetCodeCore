using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class SyncHandle
    {
        internal SynchronisableEntity sync;
        public Object obj { get; internal set; }
    }

}
