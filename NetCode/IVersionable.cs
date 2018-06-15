using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    internal interface IVersionable
    {
        /// <summary>
        /// Writes the current state into data, starting at index.
        /// </summary>
        /// <param name="data">The buffer to write to</param>
        /// <param name="index">The index to begin writing to. This value will be incremented by the number of bytes written</param>
        /// <param name="revision">The revision number describing the change.</param>
        void WriteToBuffer(byte[] data, ref int index, uint revision);

        /// <summary>
        /// Indicates how many bytes will be written to by a following WriteToBuffer call. 
        /// </summary>
        /// <returns>The number of bytes required by WriteToBuffer</returns>
        int WriteToBufferSize();

        /// <summary>
        /// Reads the data starting at index, and updates the state.
        /// NOTE: This may not be symmetrical to WriteToBuffer, as writing often includes a packet header, which typically must be read before ReadFromBuffer can be called.
        /// </summary>
        /// <param name="data">The buffer to read from</param>
        /// <param name="index">The index to begin reading from. This value will be incremented by the number of bytes read</param>
        /// <param name="revision">The revision number describing the change.</param>
        void ReadFromBuffer(byte[] data, ref int index, uint revision);

        /// <summary>
        /// Indicates whether the variable has been changed.
        /// </summary>
        bool Changed { get; } // TODO: OPTIMISE THIS OUT COMPLETELY

        /// <summary>
        /// The current revision number this variable was last updated at.
        /// </summary>
        uint Revision { get; }
    }
}
