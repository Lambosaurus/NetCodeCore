﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace NetCode.Util
{
    public static class Primitive
    {
        public const int SizeofHalf = 2;
        public const int MaxVWidthValue = (1 << 15) - 1;

        public static void WriteByte(byte[] data, ref int index, byte value)
        {
            data[index++] = value;
        }
        public static void WriteShort(byte[] data, ref int index, short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteUShort(byte[] data, ref int index, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteInt(byte[] data, ref int index, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteUInt(byte[] data, ref int index, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteLong(byte[] data, ref int index, long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteULong(byte[] data, ref int index, ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteFloat(byte[] data, ref int index, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteDouble(byte[] data, ref int index, double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteString(byte[] data, ref int index, string value)
        {
            data[index++] = (byte)(value.Length);
            foreach (char ch in value) { data[index++] = (byte)ch; }
        }
        public static void WriteHalf(byte[] data, ref int index, Half value)
        {
            byte[] bytes = Half.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void WriteByteArray(byte[] data, ref int index, byte[] value)
        {
            data[index++] = (byte)value.Length;
            foreach (byte b in value) { data[index++] = b; }
        }
        public static void WriteUShortArray(byte[] data, ref int index, ushort[] values)
        {
            data[index++] = (byte)values.Length;
            foreach (ushort value in values)
            {
                WriteUShort(data, ref index, value);
            }
        }
        public static void WriteUIntArray(byte[] data, ref int index, uint[] values)
        {
            data[index++] = (byte)values.Length;
            foreach (uint value in values)
            {
                WriteUInt(data, ref index, value);
            }
        }
        public static void WriteVWidth(byte[] data, ref int index, ushort value)
        {
            if (value >= (1 << 7))
            {
                data[index++] = (byte)((value >> 8) | (1 << 7));
            }
            data[index++] = (byte)value;
        }

        public static byte ReadByte(byte[] data, ref int index)
        {
            return data[index++];
        }
        public static short ReadShort(byte[] data, ref int index)
        {
            short value = BitConverter.ToInt16(data, index);
            index += sizeof(short);
            return value;
        }
        public static ushort ReadUShort(byte[] data, ref int index)
        {
            ushort value = BitConverter.ToUInt16(data, index);
            index += sizeof(ushort);
            return value;
        }
        public static int ReadInt(byte[] data, ref int index)
        {
            int value = BitConverter.ToInt32(data, index);
            index += sizeof(int);
            return value;
        }
        public static uint ReadUInt(byte[] data, ref int index)
        {
            uint value = BitConverter.ToUInt32(data, index);
            index += sizeof(uint);
            return value;
        }
        public static long ReadLong(byte[] data, ref int index)
        {
            long value = BitConverter.ToInt64(data, index);
            index += sizeof(long);
            return value;
        }
        public static ulong ReadULong(byte[] data, ref int index)
        {
            ulong value = BitConverter.ToUInt64(data, index);
            index += sizeof(ulong);
            return value;
        }
        public static float ReadFloat(byte[] data, ref int index)
        {
            float value = BitConverter.ToSingle(data, index);
            index += sizeof(float);
            return value;
        }
        public static double ReadDouble(byte[] data, ref int index)
        {
            double value = BitConverter.ToSingle(data, index);
            index += sizeof(double);
            return value;
        }
        public static string ReadString(byte[] data, ref int index)
        {
            byte length = data[index++];
            char[] values = new char[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (char)data[index++];
            }
            return new string(values);
        }
        public static Half ReadHalf(byte[] data, ref int index)
        {
            Half value = Half.ToHalf(data, index);
            index += SizeofHalf;
            return value;
        }
        public static byte[] ReadByteArray(byte[] data, ref int index)
        {
            byte length = data[index++];
            byte[] value = new byte[length];
            for (int i = 0; i < length; i++) { value[index] = data[index++]; }
            return value;
        }
        public static ushort[] ReadUShortArray(byte[] data, ref int index)
        {
            byte length = data[index++];
            ushort[] value = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUShort(data, ref index);
            }
            return value;
        }
        public static uint[] ReadUIntArray(byte[] data, ref int index)
        {
            byte length = data[index++];
            uint[] value = new uint[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUInt(data, ref index);
            }
            return value;
        }
        public static ushort ReadVWidth(byte[] data, ref int index)
        {
            ushort value = data[index++];
            if ((value & (1 << 7)) != 0)
            {
                value &= 0x7F;
                value <<= 8;
                value += data[index++];
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


        private static string[] SIPrefix = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
        public static string SIFormat(double value, string suffix, bool useBinaryPowers = true)
        {
            double divisor = useBinaryPowers ? 1024 : 1000;

            int power = 0;
            while ( value >= divisor)
            {
                power++;
                value /= divisor;
            }

            if (value < 10)
            {
                return string.Format("{0:0.00}{1}{2}", value, SIPrefix[power], suffix);
            }
            else if (value < 100)
            {
                return string.Format("{0:0.0}{1}{2}", value, SIPrefix[power], suffix);
            }
            else
            {
                return string.Format("{0:0.}{1}{2}", value, SIPrefix[power], suffix);
            }
        }
    }
}
