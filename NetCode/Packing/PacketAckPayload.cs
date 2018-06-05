using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.Packing
{
    public class PacketAckPayload : Payload
    {
        public const int MAX_PACKET_IDS = byte.MaxValue;
        byte PacketCount;

        public List<uint> PacketIDs;

        public PacketAckPayload() : base(PayloadType.PacketAck)
        {
            //TODO: Potentially remove this once packing is properly abstracted.
            if (PacketIDs.Count > MAX_PACKET_IDS)
            {
                throw new NetcodeOverloadedException(string.Format("May not acknowledge more than {0} packets in one payload", MAX_PACKET_IDS));
            }

            PacketCount = (byte)PacketIDs.Count;
        }

        public PacketAckPayload(List<uint> packetIDs) : base(PayloadType.PacketAck)
        {
            PacketIDs = packetIDs;
        }
        
        public override void WriteContentHeader()
        {
            Primitive.WriteByte(Data, ref Index, PacketCount);
        }

        public override void ReadContentHeader()
        {
            PacketCount = Primitive.ReadByte(Data, ref Index);
        }

        public void ReadContent()
        {
            PacketIDs = new List<uint>(PacketCount);
            for (int i = 0; i < PacketCount; i++)
            {
                uint packetID = Primitive.ReadUInt(Data, ref Index);
                PacketIDs.Add(packetID);
            }
        }

        public override int ContentHeaderSize()
        {
            return sizeof(byte);
        }

        public override bool AcknowledgementRequired()
        {
            return false;
        }

    }
}
