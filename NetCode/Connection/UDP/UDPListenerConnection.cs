using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;
using System.Net;

namespace NetCode.Connection.UDP
{
    public class UDPListenerConnection : NetworkConnection
    {
        public int Port { get; private set; }
        public IPEndPoint Destination { get; private set; }

        private UdpClient Socket;
        private IPEndPoint LastEndpoint;
        
        public UDPListenerConnection(int port)
        {
            Port = port;
            Socket = new UdpClient(Port);
            Destination = new IPEndPoint(IPAddress.Any, 0);
        }

        internal override void OnListen()
        {
            Destination.Address = IPAddress.Any;
            Destination.Port = 0;
        }

        internal override void OnConnect()
        {
            Destination = LastEndpoint;
        }
        
        protected override void SendData(byte[] data)
        {
            Socket.Send(data, data.Length, Destination);
        }

        protected override List<byte[]> RecieveData()
        {
            List<byte[]> data = new List<byte[]>();

            while (Socket.Available > 0)
            {
                byte[] packet = Socket.Receive(ref LastEndpoint);
                if (LastEndpoint.Address.GetHashCode() == Destination.Address.GetHashCode() && LastEndpoint.Port == Destination.Port)
                {
                    data.Add(packet);
                }
            }

            return data;
        }
    }
}
