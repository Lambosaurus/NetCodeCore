﻿using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.SyncField.Implementations
{
    public class SynchronisableEnum : SynchronisableField
    {
        protected byte value;
        public override void SetValue(object new_value) { value = (byte)(int)new_value; }
        public override object GetValue() { return (int)value; }
        public override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteByte(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadByte(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(byte); }
    }

    public class SynchronisableBool : SynchronisableField
    {
        protected bool value;
        public override void SetValue(object new_value) { value = (bool)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (bool)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteByte(data, ref index, value ? (byte)0x01 : (byte)0x00); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadByte(data, ref index) > 0; }
        public override void Skip(byte[] data, ref int index) { index += sizeof(byte); }
    }

    public class SynchronisableByte : SynchronisableField
    {
        protected byte value;
        public override void SetValue(object new_value) { value = (byte)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteByte(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadByte(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(byte); }
    }

    public class SynchronisableShort : SynchronisableField
    {
        protected short value;
        public override void SetValue(object new_value) { value = (short)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(short); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteShort(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadShort(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(short); }
    }

    public class SynchronisableUShort : SynchronisableField
    {
        protected ushort value;
        public override void SetValue(object new_value) { value = (ushort)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(ushort); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteUShort(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadUShort(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(ushort); }
    }

    public class SynchronisableInt : SynchronisableField
    {
        protected int value;
        public override void SetValue(object new_value) { value = (int)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(int); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteInt(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadInt(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(int); }
    }

    public class SynchronisableUInt : SynchronisableField
    {
        protected uint value;
        public override void SetValue(object new_value) { value = (uint)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(uint); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteUInt(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadUInt(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(uint); }
    }

    public class SynchronisableLong : SynchronisableField
    {
        protected long value;
        public override void SetValue(object new_value) { value = (long)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(long); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteLong(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadLong(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(long); }
    }

    public class SynchronisableULong : SynchronisableField
    {
        protected ulong value;
        public override void SetValue(object new_value) { value = (ulong)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(ulong); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteULong(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadULong(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(ulong); }
    }

    public class SynchronisableFloat : SynchronisableField
    {
        protected float value;
        public override void SetValue(object new_value) { value = (float)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(float); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteFloat(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadFloat(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(float); }
    }

    public class SynchronisableDouble : SynchronisableField
    {
        protected double value;
        public override void SetValue(object new_value) { value = (double)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (double)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(double); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteDouble(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadDouble(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += sizeof(double); }
    }

    public class SynchronisableString : SynchronisableField
    {
        protected string value;
        public override void SetValue(object new_value) { value = (string)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteToBufferSize()
        {
            if (value == null) { return sizeof(byte); }
            return Primitive.ArraySize(value.Length, sizeof(byte));
        }
        public override void Write(byte[] data, ref int index)
        {
            if (value == null) { Primitive.WriteByte(data, ref index, 0); }
            else { Primitive.WriteString(data, ref index, value); }
        }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadString(data, ref index); }
        public override void Skip(byte[] data, ref int index) { Primitive.ReadString(data, ref index); }
    }

    public class SynchronisableHalf : SynchronisableField
    {
        protected Half value;
        public override void SetValue(object new_value) { value = (Half)((float)new_value); }
        public override object GetValue() { return (float)value; }
        public override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int WriteToBufferSize() { return 2; }
        public override void Write(byte[] data, ref int index) { Primitive.WriteHalf(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadHalf(data, ref index); }
        public override void Skip(byte[] data, ref int index) { index += Primitive.SizeofHalf; }
    }

    public class SynchronisableByteArray : SynchronisableField
    {
        protected byte[] value;
        public override void SetValue(object new_value) { value = (byte[])((byte[])new_value).Clone(); }
        public override object GetValue() { return value.Clone(); }
        public override bool ValueEqual(object new_value) {  return value.SequenceEqual((byte[])new_value); }
        public override int WriteToBufferSize() { return Primitive.ArraySize(value.Length, sizeof(byte)); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteByteArray(data, ref index, value); }
        public override void Read(byte[] data, ref int index) { value = Primitive.ReadByteArray(data, ref index); }
        public override void Skip(byte[] data, ref int index) { Primitive.ReadByte(data, ref index); }
    }
}