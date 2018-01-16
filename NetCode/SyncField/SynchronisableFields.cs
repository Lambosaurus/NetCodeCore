using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using NetCode.Packets;

namespace NetCode.SyncField
{
    public abstract class SynchronisableField : IPacketReadable, IPacketWritable
    {
        public bool Changed { get; private set; } = true; // Defaults to true so value is changed when created
        public uint LastPacketID { get; private set; } = 0; // Indicates the UUID of the last packet this field was updated in

        internal void Update(object new_value)
        {
            if (!ValueEqual(new_value))
            {
                Changed = true;
                SetValue(new_value);
            }
        }

        public void WriteToPacket(byte[] data, ref int index, uint packetID)
        {
            Write(data, ref index);
            Changed = false;
            LastPacketID = packetID;
        }

        public void ReadFromPacket(byte[] data, ref int index, uint packetID)
        {
            Read(data, ref index);
            Changed = true;
            LastPacketID = packetID;
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
        public abstract int WriteSize();

        /// <summary>
        /// Writes the Synchronisable value into the packet.
        /// </summary>
        /// <param name="data"> The packet to write to </param>
        /// <param name="index"> The index to begin writing at. The index will be incremented by the number of bytes written </param>
        protected abstract void Write(byte[] data, ref int index);

        /// <summary>
        /// Reads the Synchronisable value from the packet.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index will be incremented by the number of bytes written </param>
        protected abstract void Read(byte[] data, ref int index);
    }
}
