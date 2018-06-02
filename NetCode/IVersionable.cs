using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    internal interface IVersionable
    {
        /// <summary>
        /// Writes the data into the packet buffer, starting at index.
        /// </summary>
        /// <param name="data">The buffer to write to</param>
        /// <param name="index">The index to begin writing to. This value will be incremented by the number of bytes written</param>
        /// <param name="revision">The revision number describing the change.</param>
        void PushToBuffer(byte[] data, ref int index, uint revision);

        /// <summary>
        /// Indicates how many bytes will be written to by a following PushToBuffer call. 
        /// </summary>
        /// <returns>The number of bytes required by PushToBuffer</returns>
        int PushToBufferSize();

        /// <summary>
        /// Reads the data from the packet buffer, starting at index.
        /// NOTE: This may not be symmetrical to PushToBuffer, as writing often includes a packet header, which typically must be read before PullFrombuffer can be called.
        /// </summary>
        /// <param name="data">The buffer to read from</param>
        /// <param name="index">The index to begin reading from. This value will be incremented by the number of bytes read</param>
        /// <param name="revision">The revision number describing the change.</param>
        void PullFromBuffer(byte[] data, ref int index, uint revision);
    }
}
