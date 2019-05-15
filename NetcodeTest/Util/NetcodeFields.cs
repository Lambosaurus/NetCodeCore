using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using NetCode;
using NetCode.SyncField;
using NetCode.Util;

namespace NetcodeTest.Util
{
    [EnumerateSyncField(typeof(Vector2))]
    public class SynchronisableVector2 : SynchronisableField
    {
        private Vector2 value;
        public override void SetValue(object new_value) { value = (Vector2)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (Vector2)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(float) * 2; }
        public override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteFloat(value.X);
            buffer.WriteFloat(value.Y);
        }
        public override void Read(NetBuffer buffer)
        {
            value.X = buffer.ReadFloat();
            value.Y = buffer.ReadFloat();
        }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(float) * 2; }
    }

    [EnumerateSyncField(typeof(Vector2), SyncFlags.HalfPrecision)]
    public class SynchronisableHalfVector2 : SynchronisableField
    {
        private Half x;
        private Half y;
        public override void SetValue(object new_value)
        {
            Vector2 value = (Vector2)new_value;
            x = (Half)value.X;
            y = (Half)value.Y;
        }
        public override object GetValue() { return new Vector2(x, y); }
        public override bool ValueEqual(object new_value)
        {
            Vector2 value = (Vector2)new_value;
            return x == (Half)value.X && y == (Half)value.Y;
        }
        public override int WriteToBufferSize() { return NetBuffer.SizeofHalf * 2; }
        public override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteHalf(x);
            buffer.WriteHalf(y);
        }
        public override void Read(NetBuffer buffer)
        {
            x = buffer.ReadHalf();
            y = buffer.ReadHalf();
        }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += NetBuffer.SizeofHalf * 2; }
    }

    [EnumerateSyncField(typeof(Color))]
    public class SynchronisableColor : SynchronisableField
    {
        private Color value;
        public override void SetValue(object new_value)
        {
            value = (Color)new_value;
        }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value)
        {
            return value == (Color)new_value;
        }
        public override int WriteToBufferSize() { return sizeof(uint); }
        public override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteUInt(value.PackedValue);
        }
        public override void Read(NetBuffer buffer)
        {
            value = new Color(buffer.ReadUInt());
        }
        public override void SkipFromBuffer(NetBuffer buffer) { buffer.Index += sizeof(uint); }
    }
}
