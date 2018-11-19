using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;

namespace NetCode.SyncField.Implementations
{
    [EnumerateSyncField(typeof(string))]
    public class SynchronisableString : SyncFieldExtendableHeader
    {
        protected string value;
        public override void SetValue(object new_value) { value = (string)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteToBufferSize()
        {
            if (value == null) { return SizeOfLengthHeader; }
            return SizeOfLengthHeader + (value.Length * sizeof(byte));
        }
        public override void Write(byte[] data, ref int index)
        {
            if (value == null) { Primitive.WriteNBytes(data, ref index, 0, SizeOfLengthHeader); }
            else
            {
                Primitive.WriteNBytes(data, ref index, value.Length, SizeOfLengthHeader);
                foreach (char ch in value)
                {
                    data[index++] = (byte)ch;
                }
            }
        }
        public override void Read(byte[] data, ref int index)
        {
            int length = Primitive.ReadNBytes(data, ref index, SizeOfLengthHeader);
            char[] values = new char[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (char)data[index++];
            }
            value = new string(values);
        }
        public override void Skip(byte[] data, ref int index)
        {
            int length = Primitive.ReadNBytes(data, ref index, SizeOfLengthHeader);
            index += length;
        }
    }
}
