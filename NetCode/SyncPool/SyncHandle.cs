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
            /// Indicates that an outgoing pool should no longer track updates for this object, but keeps the object available that it may be returned to live.
            /// Note that the incoming pool is not aware the handle has been suspended.
            /// </summary>
            Suspended,

            /// <summary>
            /// This object will be synchronised on the next pool revision, and then set to Suspended.
            /// </summary>
            SyncOnce,

            /// <summary>
            /// This is used to indicate that the object has been dropped by the incoming pool,
            /// or should be dropped by an outgoing pool.
            /// </summary>
            Deleted,
        };

        internal SynchronisableEntity Sync;
        public object Obj { get; internal set; }
        public SyncState State { get; set; }
        public ushort EntityID { get { return Sync.EntityID; } }

        public const ushort NullEntityID = 0;

        internal SyncHandle(SynchronisableEntity syncEntity, Object syncObject)
        {
            Sync = syncEntity;
            Obj = syncObject;
            State = SyncState.Live;
        }
    }

}
