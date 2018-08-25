using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField.Implementations
{
    public class SynchronisableReference : SynchronisableField
    {
        protected object value;
        protected ushort entityID = SyncHandle.NullEntityID;

        public override void SetValue(object new_value)
        {
            value = new_value;
        }

        public override object GetValue() { return value; }

        public override bool ValueEqual(object new_value)
        {
            return new_value == value;
        }

        protected override void PostProcess(SyncContext context)
        {
            value = null;
            SyncHandle handle = context.GetHandleByEntityID((ushort)(entityID));
            if (handle != null)
            {
                if (FieldType.IsAssignableFrom(handle.Obj.GetType()))
                {
                    value = handle.Obj;
                }
            }
        }

        protected override void PreProcess(SyncContext context)
        {
            if (value == null) { entityID = SyncHandle.NullEntityID; }
            else
            {
                SyncHandle handle = context.GetHandleByObject(value);
                entityID = (handle == null) ? SyncHandle.NullEntityID : handle.EntityID;
            }
        }

        public override int WriteToBufferSize() { return sizeof(ushort); }
        public override void Write(byte[] data, ref int index) { Primitive.WriteUShort(data, ref index, entityID); }
        public override void Read(byte[] data, ref int index) { entityID = Primitive.ReadUShort(data, ref index); }
        public override void Skip(byte[] data, ref int index) { Primitive.ReadUShort(data, ref index); }
    }
}
