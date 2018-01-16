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

        const int ID_HEADER_LENGTH = sizeof(uint);
        
        public byte[] Generate( )
        {
            int length = ID_HEADER_LENGTH;

            foreach ( Payload payload in Payloads )
            {
                length += payload.WriteSize();
            }

            byte[] data = new byte[length];
            int index = 0;

            PrimitiveSerialiser.WriteUInt(data, ref index, ID);

            foreach (Payload payload in Payloads)
            {
                payload.WriteToPacket(data, ref index);
            }

            return null;
        }
    }
}
