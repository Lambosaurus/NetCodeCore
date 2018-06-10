using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;

namespace NetCode.SyncField
{
    public class SynchronisableEnum : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)(int)new_value; }
        public override object GetValue() { return (int)value; }
        protected override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteByte(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadByte(data, ref index); }
    }

    public class SynchronisableByte : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteByte(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadByte(data, ref index); }
    }

    public class SynchronisableShort : SynchronisableField
    {
        internal short value;
        protected override void SetValue(object new_value) { value = (short)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(short); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteShort(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadShort(data, ref index); }
    }

    public class SynchronisableUShort : SynchronisableField
    {
        internal ushort value;
        protected override void SetValue(object new_value) { value = (ushort)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(ushort); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteUShort(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadUShort(data, ref index); }
    }

    public class SynchronisableInt : SynchronisableField
    {
        internal int value;
        protected override void SetValue(object new_value) { value = (int)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(int); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteInt(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadInt(data, ref index); }
    }

    public class SynchronisableUInt : SynchronisableField
    {
        internal uint value;
        protected override void SetValue(object new_value) { value = (uint)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(uint); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteUInt(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadUInt(data, ref index); }
    }

    public class SynchronisableLong : SynchronisableField
    {
        internal long value;
        protected override void SetValue(object new_value) { value = (long)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(long); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteLong(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadLong(data, ref index); }
    }

    public class SynchronisableULong : SynchronisableField
    {
        internal ulong value;
        protected override void SetValue(object new_value) { value = (ulong)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(ulong); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteULong(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadULong(data, ref index); }
    }

    public class SynchronisableFloat : SynchronisableField
    {
        internal float value;
        protected override void SetValue(object new_value) { value = (float)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(float); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteFloat(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadFloat(data, ref index); }
    }

    public class SynchronisableDouble : SynchronisableField
    {
        internal double value;
        protected override void SetValue(object new_value) { value = (double)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (double)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(double); }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteDouble(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadDouble(data, ref index); }
    }

    public class SynchronisableString : SynchronisableField
    {
        internal string value;
        protected override void SetValue(object new_value) { value = (string)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteToBufferSize() { return value.Length + 1; }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteString(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadString(data, ref index); }
    }

    public class SynchronisableHalf : SynchronisableField
    {
        internal Half value;
        protected override void SetValue(object new_value) { value = (Half)((float)new_value); }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int WriteToBufferSize() { return 2; }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteHalf(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadHalf(data, ref index); }
    }

    public class SynchronisableByteArray : SynchronisableField
    {
        internal byte[] value;
        protected override void SetValue(object new_value) { value = (byte[])((byte[])new_value).Clone(); }
        public override object GetValue() { return value.Clone(); }
        protected override bool ValueEqual(object new_value) {  return value.SequenceEqual((byte[])new_value); }
        public override int WriteToBufferSize() { return value.Length+1; }
        protected override void Write(byte[] data, ref int index) { Primitive.WriteByteArray(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitive.ReadByteArray(data, ref index); }
    }
}
