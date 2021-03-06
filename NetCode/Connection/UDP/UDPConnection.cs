﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;
using System.Net;
using NetCode.Payloads;

namespace NetCode.Connection.UDP
{
    public class UDPConnection : NetworkConnection, IDisposable
    {
        private UdpClient Socket;
        private NetworkClient.ConnectionClosedReason connectionStatus = NetworkClient.ConnectionClosedReason.None;

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

            try
            {
                while (Socket.Available > 0)
                {
                    data.Add(Socket.Receive(ref source));
                }
            }
            catch(SocketException ex)
            {
                if (ex.ErrorCode == 10054)
                {
                    connectionStatus = NetworkClient.ConnectionClosedReason.EndpointPortClosed;
                }
                else
                {
                    throw;
                }
            }

            return data;
        }

        public override void Destroy()
        {
            Socket.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)Socket).Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public override NetworkClient.ConnectionClosedReason ConnectionStatus
        { get
            {
                return connectionStatus;
            }
        }
    }
}
