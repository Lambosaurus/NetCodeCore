using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using NetCode.Payloads;

namespace NetCode.Connection.UDP
{
    public class UDPFeed : NetworkConnection
    {
        public IPEndPoint Destination { get; private set; }
        public UDPServer Host { get; private set; }
        public bool IsIncoming { get; private set; }

        private List<byte[]> IncomingData;

        public UDPFeed(UDPServer host, IPEndPoint destination, bool incoming)
        {
            Destination = destination;
            Host = host;
            IncomingData = new List<byte[]>();
            IsIncoming = incoming;
        }

        internal void FeedData(byte[] data)
        {
            IncomingData.Add(data);
        }

        protected override void SendData(byte[] data)
        {
            Host.Transmit(data, Destination);
        }

        protected override List<byte[]> RecieveData()
        {
            Host.FlushRecieve();

            if (IncomingData.Count > 0)
            {
                List<byte[]> data = IncomingData;
                IncomingData = new List<byte[]>();
                return data;
            }
            return null;
        }

        public void Close()
        {
            Host.CloseFeed(this);
            Host = null;
            IncomingData.Clear();
        }

        internal override Payload GetConnectionRequestPayload()
        {
            return UDPConnectionRequestPayload.Generate();
        }
    }
}
