using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;

namespace NetCode.Connection
{
    public class UDPConnection : NetworkConnection
    {
        protected override void SendData(byte[] data)
        {
        }

        protected override List<byte[]> RecieveData()
        {
            return null;
        }
    }
}
