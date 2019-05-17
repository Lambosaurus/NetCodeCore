using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Util;

namespace NetCode.SyncField.Implementations
{
    [EnumerateSyncField(typeof(string))]
    public class SyncFieldString : SynchronisableValue
    {
        protected string value;
        public override void SetValue(object new_value) { value = (string)new_value; }
        public override object GetValue() { return value; }
        public override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteToBufferSize()
        {
            if (value == null) return NetBuffer.SizeofVWidth(0);
            return NetBuffer.SizeofVWidth((ushort)value.Length) + (value.Length * sizeof(byte));
        }
        public override void WriteToBuffer(NetBuffer buffer)
        {
            if (value == null) { buffer.WriteVWidth(0); }
            else
            {
                buffer.WriteVWidth((ushort)value.Length);
                foreach (char ch in value)
                {
                    buffer.Data[buffer.Index++] = (byte)ch;
                }
            }
        }
        public override void ReadFromBuffer(NetBuffer buffer)
        {
            int length = buffer.ReadVWidth();
            char[] values = new char[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = (char)buffer.Data[buffer.Index++];
            }
            value = new string(values);
        }
        public override void SkipFromBuffer(NetBuffer buffer)
        {
            int length = buffer.ReadVWidth();
            buffer.Index += length;
        }
    }
}
