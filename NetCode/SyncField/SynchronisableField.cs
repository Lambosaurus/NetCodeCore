using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField
{
    public abstract class SynchronisableField
    {
        protected uint Revision { get; set; } = 0;
        public bool Synchronised { get; set; } = false;
        public bool ReferencesPending { get; protected set; } = false;

        internal abstract bool ContainsRevision(uint revision);
        internal abstract bool TrackChanges(object newValue, SyncContext context);
        internal abstract object GetChanges();

        internal abstract void ReadRevisionFromBuffer(NetBuffer buffer, SyncContext context);
        internal abstract void WriteRevisionToBuffer(NetBuffer buffer, SyncContext context);
        internal abstract int WriteRevisionToBufferSize(uint revision);
        internal abstract int WriteAllToBufferSize();
        internal abstract void WriteAllToBuffer(NetBuffer buffer);
        internal abstract void SkipRevisionFromBuffer(NetBuffer buffer);

        internal virtual void UpdateReferences(SyncContext context)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class SynchronisablePrimitive : SynchronisableField
    {
        internal override bool ContainsRevision(uint revision)
        {
            return Revision == revision;
        }

        internal override bool TrackChanges(object newValue, SyncContext context)
        {
            if (!ValueEqual(newValue))
            {
                Revision = context.Revision;
                SetValue(newValue);
                return true;
            }
            return false;
        }

        internal override object GetChanges()
        {
            Synchronised = true;
            return GetValue();
        }

        internal override void ReadRevisionFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                Read(buffer);
                Revision = context.Revision;
                Synchronised = false;
            }
            else
            {
                Skip(buffer);
            }
        }

        internal override void WriteRevisionToBuffer(NetBuffer buffer, SyncContext context)
        {
            Write(buffer);
        }

        internal override int WriteRevisionToBufferSize(uint revision)
        {
            return WriteSize();
        }

        internal override int WriteAllToBufferSize()
        {
            return WriteSize();
        }

        internal override void WriteAllToBuffer(NetBuffer buffer)
        {
            Write(buffer);
        }

        internal override void SkipRevisionFromBuffer(NetBuffer buffer)
        {
            Skip(buffer);
        }


        /// <summary>
        /// Sets the internal value of the field
        /// </summary>
        /// <param name="newValue"></param>
        public abstract void SetValue(object newValue);

        /// <summary>
        /// Gets the internal value of the field
        /// </summary>
        public abstract object GetValue();

        /// <summary>a
        /// Returns true if the new value does not match the stored value.
        /// </summary>
        /// <param name="newValue"></param>
        public abstract bool ValueEqual(object newValue);

        /// <summary>
        /// Returns the number of bytes required by Write()
        /// </summary>
        public abstract int WriteSize();

        /// <summary>
        /// Writes the Synchronisable value into the packet.
        /// </summary>
        /// <param name="data"> The packet to write to </param>
        /// <param name="index"> The index to begin writing at. The index shall be incremented by the number of bytes written </param>
        public abstract void Write(NetBuffer buffer);

        /// <summary>
        /// Reads the Synchronisable value from the packet.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index shall be incremented by the number of bytes read </param>
        public abstract void Read(NetBuffer buffer);
        
        /// <summary>
        /// Must increment the index by the number of bytes that would be read,
        /// without updating the internal state.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index shall be incremented by the number of bytes read </param>
        public abstract void Skip(NetBuffer buffer);
    }

}
