using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField.Implementations
{
    internal class SyncFieldReference : SynchronisableField
    {
        protected object value;
        protected ushort EntityID = SyncHandle.NullEntityID;
        
        protected SyncFieldDescriptor Descriptor;
        
        internal override void Initialise(SyncFieldDescriptor descriptor, byte elementDepth)
        {
            Flags = descriptor.Flags;
            Descriptor = descriptor;
        }

        public override void SetValue(object new_value)
        {
            value = new_value;
        }

        public override object GetValue() { return value; }

        public override bool ValueEqual(object new_value)
        {
            return new_value == value;
        }
        
        public override void PostProcess(SyncContext context)
        {
            value = null;
            SyncHandle handle = context.GetHandle(EntityID);

            if (handle != null)
            {
                if (Descriptor.ReferenceType.IsAssignableFrom(handle.Obj.GetType()))
                {
                    value = handle.Obj;
                }
            }
            else
            {
                PollingRequired = true;
            }
        }

        public override void PreProcess(SyncContext context)
        {
            if (value == null)
            {
                EntityID = SyncHandle.NullEntityID;
            }
            else
            {
                SyncHandle handle = context.GetHandleByObject(value);
                EntityID = (handle == null) ? SyncHandle.NullEntityID : handle.EntityID;
            }
        }

        public override void PeriodicProcess(SyncContext context)
        {
            SyncHandle handle = context.GetHandle(EntityID);
            
            if (handle != null)
            {
                if (Descriptor.ReferenceType.IsAssignableFrom(handle.Obj.GetType()))
                {
                    value = handle.Obj;
                }
                PollingRequired = false;
            }
        }

        public override int WriteToBufferSize()
        {
            return sizeof(ushort);
        }

        public override void Write(NetBuffer buffer)
        {
            buffer.WriteUShort(EntityID);
        }

        public override void Read(NetBuffer buffer)
        {
            EntityID = buffer.ReadUShort();
        }

        public override void Skip(NetBuffer buffer)
        {
            buffer.Index += sizeof(ushort);
        }   
    }
}
