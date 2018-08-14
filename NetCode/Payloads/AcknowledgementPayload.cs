using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Connection;
using NetCode.Util;

namespace NetCode.Payloads
{
    public class AcknowledgementPayload : Payload
    {
        public override PayloadType Type { get { return PayloadType.Acknowledgement; } }
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
            Primitive.WriteUIntArray(Data, ref DataIndex, PacketIDs);
        }

        public override void ReadContent()
        {
            PacketIDs = Primitive.ReadUIntArray(Data, ref DataIndex);
        }

        public override int ContentSize()
        {
            return Primitive.ArraySize(PacketIDs.Length, sizeof(uint));
        }
    }
}
