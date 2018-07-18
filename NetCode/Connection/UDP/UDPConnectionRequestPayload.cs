using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Payloads;

namespace NetCode.Connection.UDP
{
    class UDPConnectionRequestPayload : Payload
    {
        public override bool AcknowledgementRequired { get { return false; } }
        public override PayloadType Type { get { return PayloadType.UDPConnectionRequest; } }

        public UDPConnectionRequestPayload()
        {
        }

        public static UDPConnectionRequestPayload Generate()
        {
            UDPConnectionRequestPayload payload = new UDPConnectionRequestPayload();
            payload.AllocateAndWrite();
            return payload;
        }

        public override void ReadContent()
        {
        }

        public override void WriteContent()
        {
        }

        public override int ContentSize()
        {
            return 0;
        }

        public override void OnReception(NetworkClient connection)
        {
        }
    }
}
