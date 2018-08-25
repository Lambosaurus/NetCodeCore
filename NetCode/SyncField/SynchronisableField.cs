using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.SyncField
{
    public abstract class SynchronisableField
    {
        public uint Revision { get; private set; } = 0;
        public bool Synchronised { get; set; } = false;
        public SyncFlags Flags { get; set; }
        public Type FieldType { get; set; }
        
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

        internal void ReadChanges(byte[] data, ref int index, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                Read(data, ref index);

                PostProcess(context);

                Revision = context.Revision;
                Synchronised = false;
            }
            else
            {
                Skip(data, ref index);
            }
        }

        protected virtual void PostProcess(SyncContext context)
        {
        }

        protected virtual void PreProcess(SyncContext context)
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
        public abstract void Write(byte[] data, ref int index);

        /// <summary>
        /// Reads the Synchronisable value from the packet.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index shall be incremented by the number of bytes read </param>
        public abstract void Read(byte[] data, ref int index);


        /// <summary>
        /// Must increment the index by the number of bytes that would be read,
        /// without updating the internal state.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index shall be incremented by the number of bytes read </param>
        public abstract void Skip(byte[] data, ref int index);
    }

    public abstract class SynchronisableValue : SynchronisableField
    {

    }

}
