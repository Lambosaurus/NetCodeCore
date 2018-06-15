using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField
{
    public abstract class SynchronisableField : IVersionable
    {
        public bool Changed { get; private set; } = true; // Defaults to true so value is changed when created
        public uint Revision { get; private set; } = 0;

        internal void Update(object new_value)
        {
            if (!ValueEqual(new_value))
            {
                Changed = true;
                SetValue(new_value);
            }
        }

        public void WriteToBuffer(byte[] data, ref int index, uint revision)
        {
            Write(data, ref index);
            Changed = false;
            Revision = revision;
        }

        public void ReadFromBuffer(byte[] data, ref int index, uint revision)
        {
            if (revision > Revision)
            {
                Read(data, ref index);
                Changed = true;
                Revision = revision;
            }
            else
            {
                Skip(data, ref index);
            }
        }


        /// <summary>
        /// Gets the internal value of the field
        /// </summary>
        /// <param name="new_value"></param>
        protected abstract void SetValue(object new_value);

        /// <summary>
        /// Sets the internal value of the field
        /// </summary>
        public abstract object GetValue();

        /// <summary>a
        /// Returns true if the new value does not match the stored value.
        /// </summary>
        /// <param name="new_value"></param>
        protected abstract bool ValueEqual(object new_value);

        /// <summary>
        /// Returns the number of bytes required by Write()
        /// </summary>
        public abstract int WriteToBufferSize();

        /// <summary>
        /// Writes the Synchronisable value into the packet.
        /// </summary>
        /// <param name="data"> The packet to write to </param>
        /// <param name="index"> The index to begin writing at. The index shall be incremented by the number of bytes written </param>
        protected abstract void Write(byte[] data, ref int index);

        /// <summary>
        /// Reads the Synchronisable value from the packet.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index shall be incremented by the number of bytes read </param>
        protected abstract void Read(byte[] data, ref int index);


        /// <summary>
        /// Must increment the index by the number of bytes that would be read,
        /// without updating the internal state.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index shall be incremented by the number of bytes read </param>
        protected abstract void Skip(byte[] data, ref int index);
    }
}
