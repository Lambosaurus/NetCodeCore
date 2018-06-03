using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField
{
    public class SynchronisableEnum : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)(int)new_value; }
        public override object GetValue() { return (int)value; }
        protected override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int PushToBufferSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteByte(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadByte(data, ref index); }
    }

    public class SynchronisableByte : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int PushToBufferSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteByte(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadByte(data, ref index); }
    }

    public class SynchronisableShort : SynchronisableField
    {
        internal short value;
        protected override void SetValue(object new_value) { value = (short)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int PushToBufferSize() { return sizeof(short); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteShort(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadShort(data, ref index); }
    }

    public class SynchronisableUShort : SynchronisableField
    {
        internal ushort value;
        protected override void SetValue(object new_value) { value = (ushort)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int PushToBufferSize() { return sizeof(ushort); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteUShort(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadUShort(data, ref index); }
    }

    public class SynchronisableInt : SynchronisableField
    {
        internal int value;
        protected override void SetValue(object new_value) { value = (int)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int PushToBufferSize() { return sizeof(int); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteInt(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadInt(data, ref index); }
    }

    public class SynchronisableUInt : SynchronisableField
    {
        internal uint value;
        protected override void SetValue(object new_value) { value = (uint)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int PushToBufferSize() { return sizeof(uint); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteUInt(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadUInt(data, ref index); }
    }

    public class SynchronisableLong : SynchronisableField
    {
        internal long value;
        protected override void SetValue(object new_value) { value = (long)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int PushToBufferSize() { return sizeof(long); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteLong(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadLong(data, ref index); }
    }

    public class SynchronisableULong : SynchronisableField
    {
        internal ulong value;
        protected override void SetValue(object new_value) { value = (ulong)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int PushToBufferSize() { return sizeof(ulong); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteULong(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadULong(data, ref index); }
    }

    public class SynchronisableFloat : SynchronisableField
    {
        internal float value;
        protected override void SetValue(object new_value) { value = (float)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int PushToBufferSize() { return sizeof(float); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteFloat(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadFloat(data, ref index); }
    }

    public class SynchronisableDouble : SynchronisableField
    {
        internal double value;
        protected override void SetValue(object new_value) { value = (double)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (double)new_value == value; }
        public override int PushToBufferSize() { return sizeof(double); }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteDouble(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadDouble(data, ref index); }
    }

    public class SynchronisableString : SynchronisableField
    {
        internal string value;
        protected override void SetValue(object new_value) { value = (string)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int PushToBufferSize() { return value.Length + 1; }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteString(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadString(data, ref index); }
    }

    public class SynchronisableHalf : SynchronisableField
    {
        internal Half value;
        protected override void SetValue(object new_value) { value = (Half)((float)new_value); }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int PushToBufferSize() { return 2; }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteHalf(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadHalf(data, ref index); }
    }

    public class SynchronisableByteArray : SynchronisableField
    {
        internal byte[] value;
        protected override void SetValue(object new_value) { value = (byte[])((byte[])new_value).Clone(); }
        public override object GetValue() { return value.Clone(); }
        protected override bool ValueEqual(object new_value) {  return value.SequenceEqual((byte[])new_value); }
        public override int PushToBufferSize() { return value.Length+1; }
        protected override void Write(byte[] data, ref int index) { Primitives.WriteByteArray(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = Primitives.ReadByteArray(data, ref index); }
    }
}
