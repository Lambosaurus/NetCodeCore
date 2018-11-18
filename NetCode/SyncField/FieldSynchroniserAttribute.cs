using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField
{
    /// <summary>
    /// Delares SynchronisableFields 
    /// Indicates what variables and properties should be tracked and synched between SynchronisableEntities.
    /// For these variables to be synchronised, they must be visible to the class being synchronised.
    /// Ie, a private and inherted variable may not be synced.
    /// </summary>
    public class FieldSynchroniserAttribute : System.Attribute
    {
        public SyncFlags Flags { get; private set; }
        public Type FieldType { get; private set; }
        public FieldSynchroniserAttribute(Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
            FieldType = fieldType;
        }
    }
}
