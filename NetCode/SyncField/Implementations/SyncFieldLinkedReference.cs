using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.SyncField.Implementations
{
    internal class SyncFieldLinkedReference : SynchronisableField
    {
        protected object value;
        protected ushort PoolID;
        protected ushort EntityID = SyncHandle.NullEntityID;

        private Type ReferenceType;
        public SyncFieldLinkedReference(Type referenceType)
        {
            ReferenceType = referenceType;
        }

        public sealed override bool TrackChanges(object newValue, SyncContext context)
        {
            if (newValue != value)
            {
                value = newValue;
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

                Synchronised = false;
                Revision = context.Revision;
                return true;
            }
            return false;
        }

        public sealed override void UpdateReferences(SyncContext context)
        {
            SyncHandle handle = context.GetHandle(EntityID, PoolID);
            if (handle != null)
            {
                if (ReferenceType.IsAssignableFrom(handle.Obj.GetType()))
                {
                    value = handle.Obj;
                }
                Synchronised = false;
                ReferencesPending = false;
            }
        }

        public override int WriteToBufferSize()
        {
            return sizeof(ushort) + NetBuffer.SizeofVWidth(PoolID);
        }

        public override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteVWidth(PoolID);
            buffer.WriteUShort(EntityID);
        }

        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                PoolID = buffer.ReadVWidth();
                EntityID = buffer.ReadUShort();
                Revision = context.Revision;

                // In the event UpdateReferences fails, we should still fall back to null.
                value = null;
                Synchronised = false;
                ReferencesPending = true;
                UpdateReferences(context);
            }
            else
            {
                SkipFromBuffer(buffer);
            }
        }

        public sealed override object GetValue()
        {
            return value;
        }

        public sealed override void SkipFromBuffer(NetBuffer buffer)
        {
            buffer.ReadVWidth();
            buffer.Index += sizeof(ushort);
        }   
    }
}
