using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;
using System.Net;

namespace NetCode.Connection
{
    public class UDPConnection : NetworkConnection
    {
        private UdpClient Socket;

        public UDPConnection(IPAddress address, int port)
        {
            Socket = new UdpClient(port);
            Socket.Connect(address, port);
        }

        public UDPConnection(IPAddress address, int srcport, int destport)
        {
            Socket = new UdpClient(srcport);
            Socket.Connect(address, destport);
        }

        protected override void SendData(byte[] data)
        {
            Socket.Send(data, data.Length);
        }

        protected override List<byte[]> RecieveData()
        {
            IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);

            List<byte[]> data = new List<byte[]>();

            while (Socket.Available > 0)
            {
                data.Add(Socket.Receive(ref source));
            }

            return data;
        }
    }
}
