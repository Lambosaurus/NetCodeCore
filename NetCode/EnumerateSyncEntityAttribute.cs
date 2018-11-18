using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    /// <summary>
    /// Delares a synchronisable field, and the target type to synchronised.
    /// </summary>
    public class EnumerateSynchEntityAttribute : Attribute
    {
        public string Tag { get; private set; }

        /// <param name="tag">A tag that can be used to loading specific entities into NetDefinitions. Untagged items will always be loaded.</param>
        public EnumerateSynchEntityAttribute(string tag = null)
        {
            Tag = tag;
        }
    }
}
