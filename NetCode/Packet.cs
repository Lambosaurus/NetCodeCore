using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode
{
    internal class Packet
    {
        
        public PacketType packetType { get; private set; }

        public Packet(PacketType _packetType)
        {
            packetType = _packetType;
        }
    }
}
