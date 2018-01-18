using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    internal interface IBufferable
    {
        /// <summary>
        /// Writes the data into the packet buffer, starting at index.
        /// </summary>
        /// <param name="data">The buffer to write to</param>
        /// <param name="index">The index to begin writing to. This value will be incremented by the number of bytes written</param>
        /// <param name="packetID">The ID of the packet that is being written to. This is used for tracking.</param>
        void WriteToBuffer(byte[] data, ref int index, uint packetID);
        
        /// <summary>
        /// Indicates how many bytes will be written to by a following WriteToPacket call. 
        /// </summary>
        /// <returns>The number of bytes required by WriteToPacket</returns>
        int WriteSize();

        /// <summary>
        /// Reads the data from the packet buffer, starting at index.
        /// Note that this is NOT nessicarially consistant with WriteToPacket, as writing includes a packet header, which typically must be read before ReadToPacket can be called.
        /// </summary>
        /// <param name="data">The buffer to read from</param>
        /// <param name="index">The index to begin reading from. This value will be incremented by the number of bytes read</param>
        /// <param name="packetID">The ID of the packet being read from. This is used for tracking.</param>
        void ReadFromBuffer(byte[] data, ref int index, uint packetID);
    }
}
