using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.Synchronisers.Values
{
    public abstract class SynchronisableValue : Synchroniser
    {
        public sealed override bool TrackChanges(object newValue, SyncContext context)
        {
            if (!ValueEqual(newValue))
            {
                Revision = context.Revision;
                SetValue(newValue);
                return true;
            }
            return false;
        }

        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if (Revision < context.Revision)
            {
                Revision = context.Revision;
                ReadFromBuffer(buffer);
                Synchronised = false;
            }
            else
            {
                SkipFromBuffer(buffer);
            }
        }

        /// <summary>
        /// Updates the current value by parsing the supplied buffer.
        /// This may safely assume that the revision has been tested alreayd.
        /// </summary>
        public abstract void ReadFromBuffer(NetBuffer buffer);

        /// <summary>
        /// Sets the internal value of the field
        /// </summary>
        /// <param name="newValue"></param>
        public abstract void SetValue(object newValue);

        /// <summary>
        /// Returns true if the new value does not match the stored value.
        /// </summary>
        /// <param name="newValue"></param>
        public abstract bool ValueEqual(object newValue);
    }
}
