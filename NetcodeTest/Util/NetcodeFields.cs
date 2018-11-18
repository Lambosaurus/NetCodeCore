using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using NetCode;
using NetCode.SyncField;
using NetCode.Util;

namespace NetcodeTest.Util
{
    [FieldSynchroniser(typeof(Vector2))]
    public class SynchronisableVector2 : SynchronisableField
    {
        private Vector2 value;
        public override void SetValue(object new_value) { value = (Vector2)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (Vector2)new_value == value; }
        public override int WriteToBufferSize() { return sizeof(float) * 2; }
        public override void Write(byte[] data, ref int index)
        {
            Primitive.WriteFloat(data, ref index, value.X);
            Primitive.WriteFloat(data, ref index, value.Y);
        }
        public override void Read(byte[] data, ref int index)
        {
            value.X = Primitive.ReadFloat(data, ref index);
            value.Y = Primitive.ReadFloat(data, ref index);
        }
        public override void Skip(byte[] data, ref int index) { index += sizeof(float) * 2; }
    }

    [FieldSynchroniser(typeof(Vector2), SyncFlags.HalfPrecision)]
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
        public override int WriteToBufferSize() { return Primitive.SizeofHalf * 2; }
        public override void Write(byte[] data, ref int index)
        {
            Primitive.WriteHalf(data, ref index, x);
            Primitive.WriteHalf(data, ref index, y);
        }
        public override void Read(byte[] data, ref int index)
        {
            x = Primitive.ReadHalf(data, ref index);
            y = Primitive.ReadHalf(data, ref index);
        }
        public override void Skip(byte[] data, ref int index) { index += Primitive.SizeofHalf * 2; }
    }

    [FieldSynchroniser(typeof(Color))]
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
        public override void Write(byte[] data, ref int index)
        {
            Primitive.WriteUInt(data, ref index, value.PackedValue);
        }
        public override void Read(byte[] data, ref int index)
        {
            value = new Color(Primitive.ReadUInt(data, ref index));
        }
        public override void Skip(byte[] data, ref int index) { index += sizeof(uint); }
    }
}
