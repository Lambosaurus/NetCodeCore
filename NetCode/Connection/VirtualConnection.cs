using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Connection
{
    public class VirtualConnection : NetworkConnection
    {
        private List<byte[]> recievebuffer = new List<byte[]>();
        private VirtualConnection endpoint = null;

        public void Connect(VirtualConnection otherEndpoint)
        {
            endpoint = otherEndpoint;
            otherEndpoint.endpoint = this;
        }

        protected override bool Connected()
        {
            return endpoint != null;
        }

        protected override void Send(byte[] data)
        {
            endpoint.recievebuffer.Add(data);
        }

        protected override List<byte[]> Recieve()
        {
            List<byte[]> outgoingBuffer = recievebuffer;
            recievebuffer = new List<byte[]>();
            return outgoingBuffer;
        }
    }
}
