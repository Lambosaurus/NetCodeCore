using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Packets
{
    internal class Payload
    {
        public enum PayloadType { None, PoolUpdate };

        public PayloadType Type { get; private set; }
        public uint PacketID { get; private set; }

        public byte[] Data;

        public Payload(PayloadType type, uint packetID)
        {
            Type = type;
            PacketID = packetID;
        }
    }
}
