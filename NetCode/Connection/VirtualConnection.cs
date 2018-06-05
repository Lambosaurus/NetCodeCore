using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Connection
{
    public class VirtualConnection : NetworkConnection
    {
        private List<byte[]> recievebuffer = new List<byte[]>();
        private VirtualConnection endpoint;

        public void Connect(VirtualConnection otherEndpoint)
        {
            endpoint = otherEndpoint;
            otherEndpoint.endpoint = this;
        }
        
        protected override void Send(byte[] data)
        {
            endpoint.recievebuffer.Add(data);
        }
    }
}
