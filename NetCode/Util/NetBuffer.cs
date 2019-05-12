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
            WriteUShort((ushort)value);
        }
        public void WriteUShort(ushort value)
        {
            Data[Index    ] = (byte)(value >> 8);
            Data[Index + 1] = (byte)(value     );
            Index += sizeof(ushort);
        }
        public void WriteInt(int value)
        {
            WriteUInt((uint)value);
        }
        public void WriteUInt(uint value)
        {
            Data[Index    ] = (byte)(value >> 24);
            Data[Index + 1] = (byte)(value >> 16);
            Data[Index + 2] = (byte)(value >> 8 );
            Data[Index + 3] = (byte)(value      );
            Index += sizeof(uint);
        }
        public void WriteLong(long value)
        {
            WriteULong((ulong)value);
        }
        public void WriteULong(ulong value)
        {
            Data[Index    ] = (byte)(value >> 56);
            Data[Index + 1] = (byte)(value >> 48);
            Data[Index + 2] = (byte)(value >> 40);
            Data[Index + 3] = (byte)(value >> 32);
            Data[Index + 4] = (byte)(value >> 24);
            Data[Index + 5] = (byte)(value >> 16);
            Data[Index + 6] = (byte)(value >> 8 );
            Data[Index + 7] = (byte)(value      );
            Index += sizeof(long);
        }
        public unsafe void WriteFloat(float value)
        {
            WriteUInt(*(uint*)&value);
        }
        public unsafe void WriteDouble(double value)
        {
            WriteULong(*(ulong*)&value);
        }
        public void WriteString(string value)
        {
            Data[Index++] = (byte)(value.Length);
            foreach (char ch in value) { Data[Index++] = (byte)ch; }
        }
        public void WriteHalf(Half value)
        {
            ushort bits = Half.GetBits(value);
            WriteUShort(bits);
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
            return (short)ReadUShort();
        }
        public ushort ReadUShort()
        {
            ushort value = (ushort)(
                (Data[Index    ] << 8)
              | (Data[Index + 1]     )
              );

            Index += sizeof(ushort);
            return value;
        }
        public int ReadInt()
        {
            return (int)ReadUInt();
        }
        public uint ReadUInt()
        {
            uint value = (uint)(
                (Data[Index    ] << 24)
              | (Data[Index + 1] << 16)
              | (Data[Index + 2] << 8 )
              | (Data[Index + 3]      )
              );

            Index += sizeof(uint);
            return value;
        }
        public long ReadLong()
        {
            return (long)ReadULong();
        }
        public ulong ReadULong()
        {
            ulong value = (ulong)(
                (Data[Index    ] << 56)
              | (Data[Index + 1] << 48)
              | (Data[Index + 2] << 40)
              | (Data[Index + 3] << 32)
              | (Data[Index + 4] << 24)
              | (Data[Index + 5] << 16)
              | (Data[Index + 6] << 8 )
              | (Data[Index + 7]      )
              );

            Index += sizeof(ulong);
            return value;
        }
        public unsafe float ReadFloat()
        {
            uint raw = ReadUInt();
            return *(float*)&(raw);
        }
        public unsafe double ReadDouble()
        {
            ulong raw = ReadULong();
            return *(double*)&(raw);
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
            ushort bits = ReadUShort();
            return Half.ToHalf(bits);
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
