using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.Packing
{
    public class AcknowledgementPayload : Payload
    {
        public const int MAX_PACKET_IDS = byte.MaxValue;
        
        public List<uint> PacketIDs { get; private set; }
        
        private byte PacketCount;
        
        public override PayloadType Type { get { return PayloadType.Acknowledgement; } }

        public AcknowledgementPayload()
        {
            //TODO: Potentially remove this once packing is properly abstracted.
            if (PacketIDs.Count > MAX_PACKET_IDS)
            {
                throw new NetcodeOverloadedException(string.Format("May not acknowledge more than {0} packets in one payload", MAX_PACKET_IDS));
            }

            PacketCount = (byte)PacketIDs.Count;
        }

        public AcknowledgementPayload(List<uint> packetIDs)
        {
            PacketIDs = packetIDs;
        }
        
        public override void WriteContentHeader()
        {
            Primitive.WriteByte(Data, ref DataIndex, PacketCount);
        }

        public override void ReadContentHeader()
        {
            PacketCount = Primitive.ReadByte(Data, ref DataIndex);
        }

        public void ReadContent()
        {
            PacketIDs = new List<uint>(PacketCount);
            for (int i = 0; i < PacketCount; i++)
            {
                uint packetID = Primitive.ReadUInt(Data, ref DataIndex);
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
