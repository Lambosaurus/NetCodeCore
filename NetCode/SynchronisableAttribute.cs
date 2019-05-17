﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode
{
    [Flags]
    public enum SyncFlags
    {
        None = 0,

        /// <summary>
        /// Indicates the specified variable precisions can be downgraded for transport.
        /// Ie, doubles will be cast to floats, and floats cast to halfs.
        /// </summary>
        HalfPrecision = (1 << 0),

        /// <summary>
        /// The synchronised value is a timestamp derived from the local NetTime.
        /// The value will be translated into a local NetTime by the endpoint.
        /// This flag can only be used with long integers.
        /// </summary>
        Timestamp = (1 << 1),

        /// <summary>
        /// The synchronised value is a reference type, and may point to a value in the syncpool
        /// If possible the endpoint will match this reference to its appropriate local reference
        /// </summary>
        Reference = (1 << 2),

        /// <summary>
        /// For reference types this will indicate that a two byte poolID should be included.
        /// This allows the SyncEntity to be referenced if it is contained in a Linked SyncPool.
        /// </summary>
        Linked = (1 << 3),

        /// <summary>
        /// A reference type that may also be contained in a linked pool.
        /// </summary>
        LinkedReference = Reference | Linked,
    };


    /// <summary>
    /// Indicates what variables and properties should be tracked and synched between SynchronisableEntities.
    /// For these variables to be synchronised, they must be visible to the class being synchronised.
    /// Ie, a private and inherted variable may not be synced.
    /// </summary>
    public class SynchronisableAttribute : Attribute
    {
        public SyncFlags Flags { get; private set; }
        public SynchronisableAttribute(SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
        }
    }
}
