using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Util
{
    public class NetBuffer
    {
        public const int SizeofHalf = 2;
        public const int MaxVWidthValue = (1 << 15) - 1;

        private readonly int End;
        public int Start { get; private set; }
        public int Size { get; private set; }
        public int Remaining { get { return End - Index; } }
        public byte[] Data { get; private set; }
        public int Index { get; set; }

        

        public NetBuffer(byte[] data, int start, int size)
        {
            Start = start;
            Size = size;
            Index = start;
            End = Start + Size;
            Data = data;
        }
        public NetBuffer(int length) : this(new byte[length], 0, length) { }
        public NetBuffer(byte[] data) : this(data, 0, data.Length) { }


        public NetBuffer SubBuffer(int count)
        {
            NetBuffer buffer = new NetBuffer(Data, Index, count);
            Index += count;
            return buffer;
        }

        public void WriteBuffer(NetBuffer buffer)
        {
            Buffer.BlockCopy(buffer.Data, buffer.Start, Data, Index, buffer.Size);
            Index += buffer.Size;
        }

        public void WriteByte(byte value)
        {
            Data[Index++] = value;
        }
        public void WriteShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteLong(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteULong(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteString(string value)
        {
            Data[Index++] = (byte)(value.Length);
            foreach (char ch in value) { Data[Index++] = (byte)ch; }
        }
        public void WriteHalf(Half value)
        {
            byte[] bytes = Half.GetBytes(value);
            foreach (byte b in bytes) { Data[Index++] = b; }
        }
        public void WriteByteArray(byte[] value)
        {
            Data[Index++] = (byte)value.Length;
            foreach (byte b in value) { Data[Index++] = b; }
        }
        public void WriteUShortArray(ushort[] values)
        {
            Data[Index++] = (byte)values.Length;
            foreach (ushort value in values)
            {
                WriteUShort(value);
            }
        }
        public void WriteUIntArray(uint[] values)
        {
            Data[Index++] = (byte)values.Length;
            foreach (uint value in values)
            {
                WriteUInt(value);
            }
        }
        public void WriteVWidth(ushort value)
        {
            if (value >= (1 << 7))
            {
                Data[Index++] = (byte)((value >> 8) | (1 << 7));
            }
            Data[Index++] = (byte)value;
        }

        public byte ReadByte()
        {
            return Data[Index++];
        }
        public short ReadShort()
        {
            short value = BitConverter.ToInt16(Data, Index);
            Index += sizeof(short);
            return value;
        }
        public ushort ReadUShort()
        {
            ushort value = BitConverter.ToUInt16(Data, Index);
            Index += sizeof(ushort);
            return value;
        }
        public int ReadInt()
        {
            int value = BitConverter.ToInt32(Data, Index);
            Index += sizeof(int);
            return value;
        }
        public uint ReadUInt()
        {
            uint value = BitConverter.ToUInt32(Data, Index);
            Index += sizeof(uint);
            return value;
        }
        public long ReadLong()
        {
            long value = BitConverter.ToInt64(Data, Index);
            Index += sizeof(long);
            return value;
        }
        public ulong ReadULong()
        {
            ulong value = BitConverter.ToUInt64(Data, Index);
            Index += sizeof(ulong);
            return value;
        }
        public float ReadFloat()
        {
            float value = BitConverter.ToSingle(Data, Index);
            Index += sizeof(float);
            return value;
        }
        public double ReadDouble()
        {
            double value = BitConverter.ToSingle(Data, Index);
            Index += sizeof(double);
            return value;
        }
        public string ReadString()
        {
            byte length = Data[Index++];
            char[] values = new char[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (char)Data[Index++];
            }
            return new string(values);
        }
        public Half ReadHalf()
        {
            Half value = Half.ToHalf(Data, Index);
            Index += SizeofHalf;
            return value;
        }
        public byte[] ReadByteArray()
        {
            byte length = Data[Index++];
            byte[] value = new byte[length];
            for (int i = 0; i < length; i++) { value[Index] = Data[Index++]; }
            return value;
        }

        public ushort[] ReadUShortArray()
        {
            byte length = Data[Index++];
            ushort[] value = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUShort();
            }
            return value;
        }
        public uint[] ReadUIntArray()
        {
            byte length = Data[Index++];
            uint[] value = new uint[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUInt();
            }
            return value;
        }
        public ushort ReadVWidth()
        {
            ushort value = Data[Index++];
            if ((value & (1 << 7)) != 0)
            {
                value &= 0x7F;
                value <<= 8;
                value += Data[Index++];
            }
            return value;
        }

        public static int SizeOfVWidth(ushort value)
        {
            return (value >= (1 << 7)) ? sizeof(ushort) : sizeof(byte);
        }

        public static int ArraySize(int length, int itemsize)
        {
            return sizeof(byte) + (length * itemsize);
        }
    }
}
