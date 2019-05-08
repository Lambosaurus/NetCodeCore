using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Connection;
using NetCode.Util;

namespace NetCode.Payloads
{
    [EnumeratePayload]
    public class AcknowledgementPayload : Payload
    {
        public override bool AcknowledgementRequired { get { return false; } }
        public override bool ImmediateTransmitRequired { get { return true; } }

        public const int MAX_PACKET_IDS = byte.MaxValue;
        
        public uint[] PacketIDs { get; private set; }
        
        
        public AcknowledgementPayload()
        {
        }

        public static AcknowledgementPayload Generate(IEnumerable<uint> packetIDs)
        {
            if (packetIDs.Count() > MAX_PACKET_IDS)
            {
                //TODO: Potentially remove this, as the packets are being validated correctly.
                throw new NetcodeItemcountException(string.Format("May not acknowledge more than {0} packets in one payload", MAX_PACKET_IDS));
            }
            AcknowledgementPayload payload = new AcknowledgementPayload()
            {
                PacketIDs = packetIDs.ToArray()
            };
            payload.AllocateAndWrite();
            return payload;
        }

        public override void OnReception(NetworkClient client)
        {
            bool validIDFound = false;
            foreach (uint packetID in PacketIDs)
            {
                bool IDValid = client.Connection.AcknowledgePacket(packetID);
                if (IDValid)
                {
                    validIDFound = true;
                }
            }
            if (validIDFound)
            {
                client.RecieveAcknowledgement();
            }
        }
        
        public override void WriteContent()
        {
            Buffer.WriteUIntArray(PacketIDs);
        }

        public override void ReadContent()
        {
            PacketIDs = Buffer.ReadUIntArray();
        }

        public override int ContentSize()
        {
            return NetBuffer.ArraySize(PacketIDs.Length, sizeof(uint));
        }
    }
}
