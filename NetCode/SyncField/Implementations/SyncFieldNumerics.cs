using NetCode.Util;
using System;

namespace NetCode.SyncField.Implementations
{
    [EnumerateSyncField(typeof(Enum))]
    public class SynchFieldEnum : SynchronisablePrimitive
    {
        protected byte value;
        public override void SetValue(object new_value) { value = (byte)(int)new_value; }
        public override object GetValue() { return (int)value; }
        public override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        public override void Write(NetBuffer buffer) { buffer.WriteByte(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadByte(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncField(typeof(bool))]
    public class SyncFieldBool : SynchronisablePrimitive
    {
        protected bool value;
        public override void SetValue(object new_value) { value = (bool)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (bool)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        public override void Write(NetBuffer buffer) { buffer.WriteByte(value ? (byte)0x01 : (byte)0x00); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadByte() > 0; }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncField(typeof(byte))]
    public class SyncFieldByte : SynchronisablePrimitive
    {
        protected byte value;
        public override void SetValue(object new_value) { value = (byte)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        public override void Write(NetBuffer buffer) { buffer.WriteByte(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadByte(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncField(typeof(short))]
    public class SyncFieldShort : SynchronisablePrimitive
    {
        protected short value;
        public override void SetValue(object new_value) { value = (short)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int WriteSize() { return sizeof(short); }
        public override void Write(NetBuffer buffer) { buffer.WriteShort(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadShort(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(short); }
    }

    [EnumerateSyncField(typeof(ushort))]
    public class SyncFieldUShort : SynchronisablePrimitive
    {
        protected ushort value;
        public override void SetValue(object new_value) { value = (ushort)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int WriteSize() { return sizeof(ushort); }
        public override void Write(NetBuffer buffer) { buffer.WriteUShort(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadUShort(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(ushort); }
    }

    [EnumerateSyncField(typeof(int))]
    public class SyncFieldInt : SynchronisablePrimitive
    {
        protected int value;
        public override void SetValue(object new_value) { value = (int)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int WriteSize() { return sizeof(int); }
        public override void Write(NetBuffer buffer) { buffer.WriteInt(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadInt(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(int); }
    }

    [EnumerateSyncField(typeof(uint))]
    public class SyncFieldUInt : SynchronisablePrimitive
    {
        protected uint value;
        public override void SetValue(object new_value) { value = (uint)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int WriteSize() { return sizeof(uint); }
        public override void Write(NetBuffer buffer) { buffer.WriteUInt(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadUInt(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(uint); }
    }

    [EnumerateSyncField(typeof(long))]
    public class SyncFieldLong : SynchronisablePrimitive
    {
        protected long value;
        public override void SetValue(object new_value) { value = (long)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int WriteSize() { return sizeof(long); }
        public override void Write(NetBuffer buffer) { buffer.WriteLong(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadLong(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(long); }
    }

    [EnumerateSyncField(typeof(ulong))]
    public class SyncFieldULong : SynchronisablePrimitive
    {
        protected ulong value;
        public override void SetValue(object new_value) { value = (ulong)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int WriteSize() { return sizeof(ulong); }
        public override void Write(NetBuffer buffer) { buffer.WriteULong(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadULong(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(ulong); }
    }

    [EnumerateSyncField(typeof(float))]
    public class SyncFieldFloat : SynchronisablePrimitive
    {
        protected float value;
        public override void SetValue(object new_value) { value = (float)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int WriteSize() { return sizeof(float); }
        public override void Write(NetBuffer buffer) { buffer.WriteFloat(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadFloat(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(float); }
    }

    [EnumerateSyncField(typeof(double))]
    public class SyncFieldDouble : SynchronisablePrimitive
    {
        protected double value;
        public override void SetValue(object new_value) { value = (double)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (double)new_value == value; }
        public override int WriteSize() { return sizeof(double); }
        public override void Write(NetBuffer buffer) { buffer.WriteDouble(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadDouble(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += sizeof(double); }
    }

    [EnumerateSyncField(typeof(float), SyncFlags.HalfPrecision)]
    public class SyncFieldHalf : SynchronisablePrimitive
    {
        protected Half value;
        public override void SetValue(object new_value) { value = (Half)((float)new_value); }
        public override object GetValue() { return (float)value; }
        public override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int WriteSize() { return NetBuffer.SizeofHalf; }
        public override void Write(NetBuffer buffer) { buffer.WriteHalf(value); }
        public override void Read(NetBuffer buffer) { value = buffer.ReadHalf(); }
        public override void Skip(NetBuffer buffer) { buffer.Index += NetBuffer.SizeofHalf; }
    }
}
