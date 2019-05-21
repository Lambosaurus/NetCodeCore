using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Synchronisers.Entities;

namespace NetCode.SyncPool
{
    public class SyncEvent
    {
        public enum SyncState
        {
            /// <summary>
            /// The event object has not been fully synchronised because some reference fields do not match valid entities.
            /// </summary>
            PendingReferences,

            /// <summary>
            /// The event object is successfully synchronised.
            /// </summary>
            Ready,

            /// <summary>
            /// This event has been cleared by the user, and will no longer maintained by the SyncPool
            /// </summary>
            Cleared,
        };
        
        public object Obj { get; protected set; }
        internal SyncEntity Sync { get; private set; }
        public SyncState State { get; internal set; }

        internal SyncEvent(SyncEntity sync)
        {
            Obj = sync.GetValue();
            Sync = sync;
            State = sync.ReferencesPending ? SyncState.PendingReferences : SyncState.Ready;
        }

        public void Clear()
        {
            State = SyncState.Cleared;
        }
    }
}
