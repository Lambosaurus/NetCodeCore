using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField.Implementations
{
    internal class SyncFieldLinkedReference : SyncFieldReference
    {
        protected ushort PoolID;

        public override void PostProcess(SyncContext context)
        {
            value = null;
            SyncHandle handle = context.GetHandle(EntityID, PoolID);

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
                PoolID = 0;
                EntityID = SyncHandle.NullEntityID;
            }
            else
            {
                SyncHandle handle = context.GetLinkedHandleByObject(value, out ushort poolID);
                if (handle != null)
                {
                    EntityID = handle.EntityID;
                    PoolID = poolID;
                }
                else
                {
                    EntityID = SyncHandle.NullEntityID;
                    PoolID = 0;
                }
            }
        }

        public override void PeriodicProcess(SyncContext context)
        {
            SyncHandle handle = context.GetHandle(EntityID, PoolID);
            
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
            return sizeof(ushort) + Primitive.SizeOfVWidth(PoolID);
        }

        public override void Write(byte[] data, ref int index)
        {
            Primitive.WriteVWidth(data, ref index, PoolID);
            Primitive.WriteUShort(data, ref index, EntityID);
        }

        public override void Read(byte[] data, ref int index)
        {
            PoolID = Primitive.ReadVWidth(data, ref index);
            EntityID = Primitive.ReadUShort(data, ref index);
        }

        public override void Skip(byte[] data, ref int index)
        {
            index += sizeof(ushort);
        }   
    }
}
