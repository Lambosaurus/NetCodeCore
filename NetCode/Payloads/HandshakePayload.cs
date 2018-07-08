using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Connection;
using NetCode.Util;

namespace NetCode.Payloads
{
    public class HandshakePayload : Payload
    {
        public override PayloadType Type { get { return PayloadType.Handshake; } }

        public override bool AcknowledgementRequired
        {
            get {
                return State == NetworkClient.ConnectionState.Opening ||
                       State == NetworkClient.ConnectionState.Open;
            }
        }

        private NetworkClient.ConnectionState State;

        public HandshakePayload()
        {
        }

        public HandshakePayload(NetworkClient.ConnectionState state)
        {
            State = state;
            AllocateAndWrite();
        }

        public override void OnReception(NetworkClient client)
        {
            client.RecieveEndpointState(State);
        }

        public override void OnTimeout(NetworkClient client)
        {
        }

        public override int ContentSize()
        {
            return sizeof(byte);
        }

        public override void ReadContent()
        {
            State = (NetworkClient.ConnectionState)(Primitive.ReadByte(Data, ref DataIndex));
        }

        public override void WriteContent()
        {
            Primitive.WriteByte(Data, ref DataIndex, (byte)State);
        }
    }
}
