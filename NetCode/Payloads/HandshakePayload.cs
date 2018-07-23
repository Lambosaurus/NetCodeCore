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
        public override bool ImmediateTransmitRequired { get { return true; } }

        public override bool AcknowledgementRequired
        {
            get {
                return AckRequired;
            }
        }

        private NetworkClient.ConnectionState State;
        private uint LocalNetTime;
        private bool AckRequired;
        private const byte AckRequriedBit = 0x80;

        public HandshakePayload()
        {
        }

        public static HandshakePayload Generate(NetworkClient.ConnectionState state, bool ackRequired)
        {
            HandshakePayload payload = new HandshakePayload()
            {
                State = state,
                LocalNetTime = (uint)NetTime.Now(),
                AckRequired = ackRequired
            };
            payload.AllocateAndWrite();
            return payload;
        }

        public override void OnReception(NetworkClient client)
        {
            client.RecieveEndpointState(State);
            client.RecieveEndpointNetTime(LocalNetTime);
        }

        public override void OnTimeout(NetworkClient client)
        {
        }

        public override int ContentSize()
        {
            return sizeof(byte) + sizeof(uint);
        }

        public override void ReadContent()
        {
            byte header = Primitive.ReadByte(Data, ref DataIndex);
            State = (NetworkClient.ConnectionState)(header & ~AckRequriedBit);
            AckRequired = (header & AckRequriedBit) != 0;
            LocalNetTime = Primitive.ReadUInt(Data, ref DataIndex);
        }

        public override void WriteContent()
        {
            byte header = (byte)State;
            if (AckRequired) { header |= AckRequriedBit; }
            Primitive.WriteByte(Data, ref DataIndex, header );
            Primitive.WriteUInt(Data, ref DataIndex, LocalNetTime);
        }
    }
}
