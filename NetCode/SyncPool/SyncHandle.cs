using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Synchronisers.Entities;

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
        
        /// <summary>
        /// The synchronised object
        /// </summary>
        public object Obj { get; internal set; }

        /// <summary>
        /// Set this to indicate whether the OutgoingSyncPool should synchronise this device.
        /// This is only intended to be read from incoming handles.
        /// </summary>
        public SyncState State { get; set; }

        /// <summary>
        /// The EntityID identifies this handle across the network, and is unique within its SyncPool.
        /// </summary>
        public ushort EntityID { get; private set; }
        
        /// <summary>
        /// Indicates if the object was updated in the last Synchronise call.
        /// </summary>
        public bool Updated { get; internal set; }
        

        internal const ushort NullEntityID = 0;
        internal SynchronisableEntity Sync;

        internal SyncHandle(SynchronisableEntity syncEntity, object obj, ushort entityID)
        {
            Sync = syncEntity;
            Obj = obj; // This is cheaper than GetValue(), as it doesnt flush field data.
            State = SyncState.Live;
            EntityID = entityID;
        }
    }
}
