using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Synchronisers.Values
{
    /// <summary>
    /// Delares a synchronisable field, and the target type to synchronised.
    /// </summary>
    public class EnumerateSyncValueAttribute : Attribute
    {
        public SyncFlags Flags { get; private set; }
        public Type FieldType { get; private set; }
        public EnumerateSyncValueAttribute(Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
            FieldType = fieldType;
        }
    }
}
