using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;

namespace NetCode.Connection
{
    public class UDPConnection : NetworkConnection
    {
        protected override void Send(byte[] data)
        {
        }

        protected override List<byte[]> Recieve()
        {
            return null;
        }

        protected override bool Connected()
        {
            return false;
        }
    }
}
