using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;

namespace NetCode.SyncField.Implementations
{
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
}
