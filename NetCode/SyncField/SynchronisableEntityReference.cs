using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField
{
    public class SynchronisableEntityReference : SynchronisableField
    {
        private object value;
        private ushort entityID = SyncHandle.NullEntityID;

        public override void SetValue(object new_value)
        {
            value = new_value;
        }

        public override object GetValue() { return value; }

        public override bool ValueEqual(object new_value)
        {
            return new_value == value;
        }

        public override int WriteToBufferSize() { return sizeof(ushort); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteUShort(data, ref index, entityID); }
        public override void Read(byte[] data, ref int index) { entityID = Primitive.ReadUShort(data, ref index); }
        public override void Skip(byte[] data, ref int index) { Primitive.ReadUShort(data, ref index); }
    }
}
