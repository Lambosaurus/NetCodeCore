using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    [Flags]
    public enum SyncFlags { None = 0, HalfPrecisionFloats = 1 };

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
