using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;

namespace NetCode.SyncField.Implementations
{
    [EnumerateSyncField(typeof(string))]
    public class SynchronisableString : SynchronisableField
    {
        protected string value;
        public override void SetValue(object new_value) { value = (string)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteToBufferSize()
        {
            if (value == null) return Primitive.SizeOfVWidth(0);
            return Primitive.SizeOfVWidth((ushort)value.Length) + (value.Length * sizeof(byte));
        }
        public override void Write(byte[] data, ref int index)
        {
            if (value == null) { Primitive.WriteVWidth(data, ref index, 0); }
            else
            {
                Primitive.WriteVWidth(data, ref index, (ushort)value.Length);
                foreach (char ch in value)
                {
                    data[index++] = (byte)ch;
                }
            }
        }
        public override void Read(byte[] data, ref int index)
        {
            int length = Primitive.ReadVWidth(data, ref index);
            char[] values = new char[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (char)data[index++];
            }
            value = new string(values);
        }
        public override void Skip(byte[] data, ref int index)
        {
            int length = Primitive.ReadVWidth(data, ref index);
            index += length;
        }
    }
}
