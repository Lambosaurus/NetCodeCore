using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField
{
    /// <summary>
    /// Delares a synchronisable field, and the target type to synchronised.
    /// </summary>
    public class NetSynchronisableFieldAttribute : System.Attribute
    {
        public SyncFlags Flags { get; private set; }
        public Type FieldType { get; private set; }
        public NetSynchronisableFieldAttribute(Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
            FieldType = fieldType;
        }
    }
}
