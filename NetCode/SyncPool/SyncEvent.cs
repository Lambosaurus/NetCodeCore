using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NetCode.SyncPool
{
    public class SyncEvent
    {
        public long Timestamp { get; protected set; }
        public object Obj { get; protected set; }
        
        public SyncEvent(object obj, long timestamp)
        {
            Obj = obj;
            Timestamp = timestamp;
        }
        
    }
}
