using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Payload
{
    public class Packet
    {
        public enum PayloadType { None, SyncPoolUpdate };


        public uint PacketID { get; private set; }
        byte[] data;
        int index;
        
        public Packet(uint packetID)
        {
            PacketID = packetID;
        }
    }
}
