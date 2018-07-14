using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;
using System.Net;

namespace NetCode.Connection.UDP
{
    public class UDPDirectConnection : NetworkConnection
    {
        private UdpClient Socket;

        public UDPDirectConnection(IPAddress address, int port)
        {
            Socket = new UdpClient(port);
            Socket.Connect(address, port);
        }

        public UDPDirectConnection(IPAddress address, int srcport, int destport)
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
