using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Connection;
using NetCode.Util;

namespace NetCode.Packing
{
    public class AcknowledgementPayload : Payload
    {
        public override PayloadType Type { get { return PayloadType.Acknowledgement; } }
        public override bool AcknowledgementRequired { get { return false; } }

        public const int MAX_PACKET_IDS = byte.MaxValue;
        
        public uint[] PacketIDs { get; private set; }
        
        
        public AcknowledgementPayload()
        {
        }

        public AcknowledgementPayload(IEnumerable<uint> packetIDs)
        {
            //TODO: Potentially remove this once packing is properly abstracted.
            if (packetIDs.Count() > MAX_PACKET_IDS)
            {
                throw new NetcodeOverloadedException(string.Format("May not acknowledge more than {0} packets in one payload", MAX_PACKET_IDS));
            }
            PacketIDs = packetIDs.ToArray();
            AllocateAndWrite();
        }

        public override void OnReception(NetworkClient client)
        {
            foreach (uint packetID in PacketIDs)
            {
                client.Connection.AcknowledgePacket(packetID);
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
