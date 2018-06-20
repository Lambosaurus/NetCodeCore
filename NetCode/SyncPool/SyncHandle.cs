using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class SyncHandle
    {

        public enum SyncState {
            Live,

            /// <summary>
            /// Indicates that an outgoing connection should no longer track updates for this object, but keeps the object such that it may be returned to live.
            /// </summary>
            Suspended,

            /// <summary>
            /// This is used to indicate that the object has been dropped by the incoming pool,
            /// or should be dropped by an outgoing pool.
            /// </summary>
            Deleted,
        };

        internal SynchronisableEntity Sync;
        public Object Obj { get; internal set; }
        public SyncState State { get; set; }
        public ushort EntityID { get { return Sync.EntityID; } }

        internal SyncHandle(SynchronisableEntity syncEntity, Object syncObject)
        {
            Sync = syncEntity;
            Obj = syncObject;
            State = SyncState.Live;
        }
    }

}
