using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Reflection.Emit;

namespace NetCode
{
    internal static class ConstructorGenerator
    {
        public static Delegate GenerateConstructorDelegate(Type type, Type delegateType)
        {
            ConstructorInfo constructor = type.GetConstructor(new Type[0]);

            // Create the dynamic method
            DynamicMethod method =
                new DynamicMethod(
                    string.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")),
                    constructor.DeclaringType,
                    new Type[0],
                    true
                    );

            // Create the il
            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            // Return the delegate
            return method.CreateDelegate(delegateType);
        }
    }

    public static class PrimitiveSerialiser
    {
        public static void Write(byte[] data, ref int index, byte value)
        {
            data[index++] = value;
        }
        public static void Write(byte[] data, ref int index, short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void Write(byte[] data, ref int index, ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void Write(byte[] data, ref int index, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void Write(byte[] data, ref int index, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void Write(byte[] data, ref int index, long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void Write(byte[] data, ref int index, ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }
        public static void Write(byte[] data, ref int index, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
        }

        public static void Write(byte[] data, ref int index, string value)
        {
            data[index++] = (byte)value.Length;
            foreach (char ch in value) { data[index++] = (byte)ch; }
        }
        public static void Write(byte[] data, ref int index, Half value)
        {
            byte[] bytes = Half.GetBytes(value);
            foreach (byte b in bytes) { data[index++] = b; }
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
            index += 2; //sizeof(Half);
            return value;
        }
        
    }

}
