using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    [Flags]
    public enum SyncFlags { None = 0, HalfPrecisionFloats = 1 };

    public class SynchronisableAttribute : System.Attribute
    {
        public SyncFlags Flags { get; private set; }
        public SynchronisableAttribute(SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
        }
    }
}
