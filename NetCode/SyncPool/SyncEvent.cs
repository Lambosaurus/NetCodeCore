using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class SyncEvent
    {
        public enum SyncState
        {
            PendingReferences,
            Ready,            
            Cleared,
        };
        
        public object Obj { get; protected set; }
        internal SynchronisableEntity Sync { get; private set; }
        public SyncState State { get; internal set; }

        public SyncEvent(SynchronisableEntity sync, object obj)
        {
            Obj = obj;
            Sync = sync;
            State = sync.PollingRequired ? SyncState.PendingReferences : SyncState.Ready;
        }

        public void Clear()
        {
            State = SyncState.Cleared;
        }
    }
}
