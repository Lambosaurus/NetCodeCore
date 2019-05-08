using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField
{
    public abstract class SynchronisableField
    {
        public uint Revision { get; private set; } = 0;
        public bool Synchronised { get; set; } = false;
        public bool PollingRequired { get; protected set; } = false;
        public SyncFlags Flags { get; internal set; }

        internal virtual void Initialise(SyncFieldDescriptor descriptor, byte elementDepth)
        {
            Flags = descriptor.Flags;
        }
        
        internal bool TrackChanges(object newValue, SyncContext context)
        {
            if (!ValueEqual(newValue))
            {
                Revision = context.Revision;
                SetValue(newValue);
                PreProcess(context);
                return true;
            }
            return false;
        }

        internal void ReadChanges(NetBuffer buffer, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                Read(buffer);

                PostProcess(context);

                Revision = context.Revision;
                Synchronised = false;
            }
            else
            {
                Skip(buffer);
            }
        }

        /// <summary>
        /// Will be called while PollingRequired is true.
        /// </summary>
        /// <param name="context"></param>
        public virtual void PeriodicProcess(SyncContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performed a value has been read from a payload
        /// </summary>
        /// <param name="context">The destination SyncPool context</param>
        public virtual void PostProcess(SyncContext context)
        {
        }

        /// <summary>
        /// Performed a value has been read from the entity
        /// </summary>
        /// <param name="context">The source SyncPool context</param>
        public virtual void PreProcess(SyncContext context)
        {
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
        public abstract int WriteToBufferSize();

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
