using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.Packet
{
    public class Packet
    {
        public List<Datagram> Datagrams { get; private set; }

        public bool DecodingError { get; private set; } = false;

        public Packet()
        {
            Datagrams = new List<Datagram>();
        }

        public byte[] Encode()
        {
            int size = 0;
            foreach (Datagram datagram in Datagrams)
            {
                size += datagram.Size;
            }

            int index = 0;
            byte[] data = new byte[size];

            foreach (Datagram datagram in Datagrams)
            {
                datagram.CopyContent(data, ref index);
                datagram.ClearContent();
            }

            return data;
        }
        
        public static Packet Decode(byte[] data)
        {
            Packet packet = new Packet();

            int index = 0;
            int length = data.Length;
            while (index + Datagram.DatagramHeaderSize < length)
            {
                int refIndex = index;
                Datagram.ReadDatagramHeader(data, ref refIndex, out Datagram.Datatype datatype, out int size);

                Datagram datagram = null;
                
                if (index + size > length)
                {
                    break;
                }

                switch (datatype)
                {
                    case (Datagram.Datatype.PoolRevision):
                        datagram = new PoolRevisionDatagram();
                        break;
                }

                if (datagram == null)
                {
                    break;
                }

                datagram.AllocateFromExisting(data, index, size);
                index += size;

                packet.Datagrams.Add(datagram);
            }

            if (index != length)
            {
                packet.DecodingError = true;
            }

            return packet;
        }
    }
}
