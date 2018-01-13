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
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteByte(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadByte(data, ref index); }
    }

    public class SynchronisableByte : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteByte(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadByte(data, ref index); }
    }

    public class SynchronisableShort : SynchronisableField
    {
        internal short value;
        protected override void SetValue(object new_value) { value = (short)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int WriteSize() { return sizeof(short); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteShort(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadShort(data, ref index); }
    }

    public class SynchronisableUShort : SynchronisableField
    {
        internal ushort value;
        protected override void SetValue(object new_value) { value = (ushort)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int WriteSize() { return sizeof(ushort); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteUShort(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadUShort(data, ref index); }
    }

    public class SynchronisableInt : SynchronisableField
    {
        internal int value;
        protected override void SetValue(object new_value) { value = (int)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int WriteSize() { return sizeof(int); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteInt(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadInt(data, ref index); }
    }

    public class SynchronisableUInt : SynchronisableField
    {
        internal uint value;
        protected override void SetValue(object new_value) { value = (uint)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int WriteSize() { return sizeof(uint); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteUInt(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadUInt(data, ref index); }
    }

    public class SynchronisableLong : SynchronisableField
    {
        internal long value;
        protected override void SetValue(object new_value) { value = (long)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int WriteSize() { return sizeof(long); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteLong(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadLong(data, ref index); }
    }

    public class SynchronisableULong : SynchronisableField
    {
        internal ulong value;
        protected override void SetValue(object new_value) { value = (ulong)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int WriteSize() { return sizeof(ulong); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteULong(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadULong(data, ref index); }
    }

    public class SynchronisableFloat : SynchronisableField
    {
        internal float value;
        protected override void SetValue(object new_value) { value = (float)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int WriteSize() { return sizeof(float); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteFloat(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadFloat(data, ref index); }
    }

    public class SynchronisableString : SynchronisableField
    {
        internal string value;
        protected override void SetValue(object new_value) { value = (string)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteSize() { return value.Length + 1; }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteString(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadString(data, ref index); }
    }

    public class SynchronisableHalf : SynchronisableField
    {
        internal Half value;
        protected override void SetValue(object new_value) { value = (Half)((float)new_value); }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int WriteSize() { return 2; }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.WriteHalf(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadHalf(data, ref index); }
    }
}
