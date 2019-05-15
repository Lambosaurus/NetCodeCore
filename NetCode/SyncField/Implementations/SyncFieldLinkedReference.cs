using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField.Implementations
{
    internal class SyncFieldLinkedReference<T> : SyncFieldReference<T>
    {
        protected ushort PoolID;

        public override void PostProcess(SyncContext context)
        {
            value = null;
            SyncHandle handle = context.GetHandle(EntityID, PoolID);

            if (handle != null)
            {
                if (typeof(T).IsAssignableFrom(handle.Obj.GetType()))
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
                if (typeof(T).IsAssignableFrom(handle.Obj.GetType()))
                {
                    value = handle.Obj;
                }
                PollingRequired = false;
            }
        }

        public override int WriteToBufferSize()
        {
            return sizeof(ushort) + NetBuffer.SizeOfVWidth(PoolID);
        }

        public override void Write(NetBuffer buffer)
        {
            buffer.WriteVWidth(PoolID);
            buffer.WriteUShort(EntityID);
        }

        public override void Read(NetBuffer buffer)
        {
            PoolID = buffer.ReadVWidth();
            EntityID = buffer.ReadUShort();
        }

        public override void Skip(NetBuffer buffer)
        {
            buffer.ReadVWidth();
            buffer.Index += sizeof(ushort);
        }   
    }
}
