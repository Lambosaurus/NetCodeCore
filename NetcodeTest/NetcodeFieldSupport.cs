using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using NetCode;
using NetCode.SyncField;

namespace NetcodeTest
{

    public static class NetcodeFieldSupport
    {
        public static void RegisterCustomFields(NetCodeManager netcode)
        {
            netcode.RegisterField(typeof(SynchronisableVector2), typeof(Vector2));
            netcode.RegisterField(typeof(SynchronisableHalfVector2), typeof(Vector2), SyncFlags.HalfPrecisionFloats);
        }
    }

    public class SynchronisableVector2 : SynchronisableField
    {
        internal Vector2 value;
        protected override void SetValue(object new_value) { value = (Vector2)new_value; }
        public override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (Vector2)new_value == value; }
        public override int WriteSize() { return sizeof(float) * 2; }
        protected override void Write(byte[] data, ref int index)
        {
            PrimitiveSerialiser.WriteFloat(data, ref index, value.X);
            PrimitiveSerialiser.WriteFloat(data, ref index, value.Y);
        }
        protected override void Read(byte[] data, ref int index)
        {
            value.X = PrimitiveSerialiser.ReadFloat(data, ref index);
            value.Y = PrimitiveSerialiser.ReadFloat(data, ref index);
        }
    }

    public class SynchronisableHalfVector2 : SynchronisableField
    {
        internal Half x;
        internal Half y;
        protected override void SetValue(object new_value)
        {
            Vector2 value = (Vector2)new_value;
            x = (Half)value.X;
            y = (Half)value.Y;
        }
        public override object GetValue() { return new Vector2(x, y); }
        protected override bool ValueEqual(object new_value)
        {
            Vector2 value = (Vector2)new_value;
            return x == (Half)value.X && y == (Half)value.Y;
        }
        public override int WriteSize() { return 4; }
        protected override void Write(byte[] data, ref int index)
        {
            PrimitiveSerialiser.WriteHalf(data, ref index, x);
            PrimitiveSerialiser.WriteHalf(data, ref index, y);
        }
        protected override void Read(byte[] data, ref int index)
        {
            x = PrimitiveSerialiser.ReadHalf(data, ref index);
            y = PrimitiveSerialiser.ReadHalf(data, ref index);
        }
    }
}
