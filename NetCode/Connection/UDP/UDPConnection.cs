using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;
using System.Net;
using NetCode.Payloads;

namespace NetCode.Connection.UDP
{
    public class UDPConnection : NetworkConnection
    {
        private UdpClient Socket;

        public UDPConnection(IPAddress address, int port) : this(address, port, port)
        {
        }

        public UDPConnection(IPAddress address, int destport, int srcport)
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

        internal override Payload GetConnectionRequestPayload()
        {
            return UDPConnectionRequestPayload.Generate();
        }
    }
}
