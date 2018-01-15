using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Packets
{
    internal class Packet
    {
        public List<Payload> Payloads { get; private set; }
        public uint ID { get; private set; }

        public Packet(uint id)
        {
            Payloads = new List<Payload>();
            ID = id;
        }
    }
}
