using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField
{
    /// <summary>
    /// Delares a synchronisable field, and the target type to synchronised.
    /// </summary>
    public class EnumerateSyncFieldAttribute : Attribute
    {
        public SyncFlags Flags { get; private set; }
        public Type FieldType { get; private set; }
        public EnumerateSyncFieldAttribute(Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
            FieldType = fieldType;
        }
    }
}
