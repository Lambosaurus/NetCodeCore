using System;
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
        HalfPrecisionFloats = (1 << 0),

        /// <summary>
        /// The synchronised value is a timestamp derived from the local NetTime.
        /// The value will be translated into a local NetTime by the endpoint.
        /// </summary>
        Timestamp = (1 << 1),

        /// <summary>
        /// The synchronised value is a reference type, and may point to a value in the syncpool
        /// If so, the endpoint will match this reference to its appropriate local reference
        /// </summary>
        Reference = (1 << 2),
    };

    /// <summary>
    /// Indicates what variables and properties should be tracked and synched between SynchronisableEntities.
    /// For these variables to be synchronised, they must be visible to the class being synchronised.
    /// Ie, a private and inherted variable may not be synced.
    /// </summary>
    public class SynchronisableAttribute : System.Attribute
    {
        public SyncFlags Flags { get; private set; }
        public SynchronisableAttribute(SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
        }
    }
}
