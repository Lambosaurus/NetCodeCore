using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Packet
{
    public abstract class Datagram
    {
        public enum Datatype { None, PoolRevision }
        public Datatype Type;

        internal byte[] Data;
        internal int Size;
        internal int Start;
        internal int Index;

        public Datagram(Datatype type)
        {
            Type = type;
        }


        public const int DatagramHeaderSize = sizeof(ushort) + sizeof(byte);
    
        public void WriteDatagramHeader()
        {
            Primitives.WriteByte(Data, ref Index, (byte)Type);
            Primitives.WriteUShort(Data, ref Index, (ushort)Size);
        }
        
        public static void ReadDatagramHeader( byte[] data, ref int index, out Datatype datatype, out int size )
        {
            datatype = (Datatype)Primitives.ReadByte(data, ref index);
            size = Primitives.ReadUShort(data, ref index);
        }

        public abstract void WriteContentHeader();
        public abstract void ReadContentHeader();
        public abstract int ContentHeaderSize();
        
        public void AllocateContent(int contentSize)
        {
            Size = contentSize + ContentHeaderSize() + DatagramHeaderSize;
            Data = new byte[Size];
            Start = 0;
            Index = Start;

            WriteDatagramHeader();
            WriteContentHeader();
        }

        public void AllocateFromExisting(byte[] data, int start, int size)
        {
            Size = size;
            Data = data;
            Start = start;
            Index = Start + DatagramHeaderSize; // Skip datagram header.

            ReadContentHeader();
        }

        public void CopyContent(byte[] buffer, ref int index)
        {
            Buffer.BlockCopy(Data, Start, buffer, index, Size);
            index += Size;
        }

        public void ClearContent()
        {
            Data = null;
        }
    }
}
