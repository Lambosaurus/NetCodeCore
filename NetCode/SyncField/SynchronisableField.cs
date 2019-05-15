using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField
{
    public abstract class SynchronisableField
    {
        /// <summary>
        /// Returns the number of bytes required by Write()
        /// </summary>
        protected uint Revision { get; set; } = 0;
        public bool Synchronised { get; set; } = false;
        public bool ReferencesPending { get; protected set; } = false;

        //TODO: Find a WAY more elegant solution to this.
        internal virtual void Initialise(SyncFieldDescriptor descriptor, byte elementDepth) { }

        /// <summary>
        /// Returns true if the current data reflects changes made at the given revision
        /// This is used to indicate whether this data needs to be resent if a specific revision payload needs to be regenerated.
        /// </summary>
        public virtual bool ContainsRevision(uint revision)
        {
            return Revision == revision;
        }

        /// <summary>
        /// Updates the current value state with the supplied value.
        /// Returns true if the new value has triggered an uprevision, and therefore must be synchronised.
        /// </summary>
        public abstract bool TrackChanges(object newValue, SyncContext context);

        /// <summary>
        /// The current value tracked by this object
        /// </summary>
        public abstract object GetValue();

        /// <summary>
        /// Updates the current object from the supplied buffer.
        /// The revision can be found in the required context, and this function is expected to gracefully content of old or mixed revisions.
        /// Synchronised should be cleared if the tracked value has been updated as a result.
        /// </summary>
        public abstract void ReadFromBuffer(NetBuffer buffer, SyncContext context);
        

        /// <summary>
        /// Writes the enture value to the supplied buffer
        /// </summary>
        public abstract void WriteToBuffer(NetBuffer buffer);

        /// <summary>
        /// Writes the current value to the supplied buffer.
        /// The requested revision is supplied within the SyncContext
        /// This must only be overridden if the write is revision dependant.
        /// </summary>
        public virtual void WriteToBuffer(NetBuffer buffer, SyncContext context)
        {
            WriteToBuffer(buffer);
        }

        /// <summary>
        /// Indicate the bytes required to write the given revision. 
        /// This must only be overridden if the size is revision dependant
        /// </summary>
        public virtual int WriteToBufferSize(uint revision)
        {
            return WriteToBufferSize();
        }

        /// <summary>
        /// Indicates the bytes required to write the current value.
        /// </summary>
        public abstract int WriteToBufferSize();

        /// <summary>
        /// Ensures the buffer head is in the correct position as if the content had been read, but without updating the internal state.
        /// </summary>
        public abstract void SkipFromBuffer(NetBuffer buffer);

        /// <summary>
        /// Gives the object an oppertunity to attempt to resolve any missing SyncReferences.
        /// This will be called periodically while ReferencesPending is true.
        /// This functions should clear ReferencesPending to indicate success (or sufficiently extreme failure)
        /// </summary>
        public virtual void UpdateReferences(SyncContext context)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class SynchronisableValue : SynchronisableField
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
