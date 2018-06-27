using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.Connection;

namespace NetCode.Packing
{
    public class KeepAlivePayload : Payload
    {
        public override PayloadType Type { get { return PayloadType.KeepAlive; } }
        public override bool AcknowledgementRequired { get { return true; } }

        public KeepAlivePayload()
        {
        }

        public static KeepAlivePayload Allocated()
        {
            KeepAlivePayload payload = new KeepAlivePayload();
            payload.AllocateAndWrite();
            return payload;
        }

        public override void OnReception(NetworkConnection connection)
        {
        }

        public override void OnTimeout(NetworkConnection connection)
        {
        }

        public override int ContentSize()
        {
            return 0;
        }

        public override void ReadContent()
        {
        }

        public override void WriteContent()
        {
        }
    }
}
