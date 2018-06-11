using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Connection;
using NetCode.Util;

namespace NetCode.Packing
{
    public class AcknowledgementPayload : Payload
    {
        public const int MAX_PACKET_IDS = byte.MaxValue;
        
        public uint[] PacketIDs { get; private set; }
        
        private byte PacketCount;
        
        public override PayloadType Type { get { return PayloadType.Acknowledgement; } }
        public override bool AcknowledgementRequired { get { return false; } }

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
            PacketCount = (byte)PacketIDs.Length;

            WriteContent();
        }

        public override void OnReception(NetworkConnection connection)
        {
            ReadContent();
            foreach (uint packetID in PacketIDs)
            {
                connection.AcknowledgePacket(packetID);
            }
        }
        
        public override void WriteContentHeader()
        {
            Primitive.WriteByte(Data, ref DataIndex, PacketCount);
        }

        public override void ReadContentHeader()
        {
            PacketCount = Primitive.ReadByte(Data, ref DataIndex);
        }

        public override int ContentHeaderSize()
        {
            return sizeof(byte);
        }

        private void ReadContent()
        {
            PacketIDs = new uint[PacketCount];
            for (int i = 0; i < PacketCount; i++)
            {
                uint packetID = Primitive.ReadUInt(Data, ref DataIndex);
                PacketIDs[i] = packetID;
            }
        }

        private void WriteContent()
        {
            AllocateContent(PacketCount * sizeof(uint));
            foreach (uint packetID in PacketIDs)
            {
                Primitive.WriteUInt(Data, ref DataIndex, packetID);
            }
        }
    }
}
