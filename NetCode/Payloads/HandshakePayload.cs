using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Connection;
using NetCode.Util;

namespace NetCode.Payloads
{
    [EnumeratePayload]
    public class HandshakePayload : Payload
    {
        public override bool ImmediateTransmitRequired { get { return true; } }

        public override bool AcknowledgementRequired
        {
            get {
                return AckRequired;
            }
        }

        public NetworkClient.ConnectionState State { get; private set; }
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
            long timestamp = NetTime.Now();
            client.Connection.Stats.RecordNetTimeOffset(LocalNetTime - timestamp, timestamp);
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
            byte header = Buffer.ReadByte();
            State = (NetworkClient.ConnectionState)(header & ~AckRequriedBit);
            AckRequired = (header & AckRequriedBit) != 0;
            LocalNetTime = Buffer.ReadUInt();
        }

        public override void WriteContent()
        {
            byte header = (byte)State;
            if (AckRequired) { header |= AckRequriedBit; }
            Buffer.WriteByte(header );
            Buffer.WriteUInt(LocalNetTime);
        }
    }
}
