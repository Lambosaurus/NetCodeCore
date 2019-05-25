using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Synchronisers.Values
{
    [EnumerateSyncValue(typeof(Enum))]
    public class SyncValueEnum : SynchronisableValue
    {
        protected byte value;
        public override void SetValue(object new_value) { value = (byte)(int)new_value; }
        public override object GetValue() { return (int)value; }
        public override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteByte(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadByte(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncValue(typeof(bool))]
    public class SyncValueBool : SynchronisableValue
    {
        protected bool value;
        public override void SetValue(object new_value) { value = (bool)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (bool)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteByte(value ? (byte)0x01 : (byte)0x00); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadByte() > 0; }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncValue(typeof(byte))]
    public class SyncValueByte : SynchronisableValue
    {
        protected byte value;
        public override void SetValue(object new_value) { value = (byte)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteByte(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadByte(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncValue(typeof(char))]
    public class SyncValueChar : SynchronisableValue
    {
        protected char value;
        public override void SetValue(object new_value) { value = (char)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (char)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(byte); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteByte((byte)value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = (char)buffer.ReadByte(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(byte); }
    }

    [EnumerateSyncValue(typeof(short))]
    public class SyncValueShort : SynchronisableValue
    {
        protected short value;
        public override void SetValue(object new_value) { value = (short)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(short); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteShort(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadShort(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(short); }
    }

    [EnumerateSyncValue(typeof(ushort))]
    public class SyncValueUShort : SynchronisableValue
    {
        protected ushort value;
        public override void SetValue(object new_value) { value = (ushort)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(ushort); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteUShort(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadUShort(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(ushort); }
    }

    [EnumerateSyncValue(typeof(int))]
    public class SyncValueInt : SynchronisableValue
    {
        protected int value;
        public override void SetValue(object new_value) { value = (int)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(int); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteInt(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadInt(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(int); }
    }

    [EnumerateSyncValue(typeof(uint))]
    public class SyncValueUInt : SynchronisableValue
    {
        protected uint value;
        public override void SetValue(object new_value) { value = (uint)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(uint); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteUInt(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadUInt(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(uint); }
    }

    [EnumerateSyncValue(typeof(long))]
    public class SyncValueLong : SynchronisableValue
    {
        protected long value;
        public override void SetValue(object new_value) { value = (long)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(long); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteLong(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadLong(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(long); }
    }

    [EnumerateSyncValue(typeof(ulong))]
    public class SyncValueULong : SynchronisableValue
    {
        protected ulong value;
        public override void SetValue(object new_value) { value = (ulong)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(ulong); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteULong(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadULong(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(ulong); }
    }

    [EnumerateSyncValue(typeof(float))]
    public class SyncValueFloat : SynchronisableValue
    {
        protected float value;
        public override void SetValue(object new_value) { value = (float)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(float); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteFloat(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadFloat(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(float); }
    }

    [EnumerateSyncValue(typeof(double))]
    public class SyncValueDouble : SynchronisableValue
    {
        protected double value;
        public override void SetValue(object new_value) { value = (double)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (double)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(double); }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteDouble(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadDouble(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(double); }
    }

    [EnumerateSyncValue(typeof(float), SyncFlags.HalfPrecision)]
    public class SyncValueHalf : SynchronisableValue
    {
        protected Half value;
        public override void SetValue(object new_value) { value = (Half)((float)new_value); }
        public override object GetValue() { return (float)value; }
        public override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int WriteToBufferSize() { return NetBuffer.SizeofHalf; }
        public override void WriteToBuffer(NetBuffer buffer) { buffer.WriteHalf(value); }
        public override void ReadFromBuffer(NetBuffer buffer) { value = buffer.ReadHalf(); }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += NetBuffer.SizeofHalf; }
    }
}
