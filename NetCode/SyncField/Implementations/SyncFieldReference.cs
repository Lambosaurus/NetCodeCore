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
        
        internal void Initialise(SyncFieldDescriptor descriptor, byte elementDepth)
        {
            Descriptor = descriptor;
        }

        internal override bool ContainsRevision(uint revision)
        {
            return Revision == revision;
        }

        internal override bool TrackChanges(object newValue, SyncContext context)
        {
            if (newValue != value)
            {
                Revision = context.Revision;
                value = newValue;
                if (value == null)
                {
                    EntityID = SyncHandle.NullEntityID;
                }
                else
                {
                    SyncHandle handle = context.GetHandleByObject(value);
                    EntityID = (handle != null) ? handle.EntityID : SyncHandle.NullEntityID;
                }

                return true;
            }
            return false;
        }

        internal override void UpdateReferences(SyncContext context)
        {
            SyncHandle handle = context.GetHandle(EntityID);
            
            if (handle != null)
            {
                if (Descriptor.ReferenceType.IsAssignableFrom(handle.Obj.GetType()))
                {
                    value = handle.Obj;
                }
                Synchronised = false;
                ReferencesPending = false;
            }
        }

        internal override int WriteRevisionToBufferSize(uint revision)
        {
            return sizeof(ushort);
        }

        internal override int WriteAllToBufferSize()
        {
            return sizeof(ushort);
        }

        internal override void WriteRevisionToBuffer(NetBuffer buffer, SyncContext context)
        {
            buffer.WriteUShort(EntityID);
        }

        internal override void ReadRevisionFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                EntityID = buffer.ReadUShort();
                Revision = context.Revision;
                Synchronised = false;

                value = null;
                UpdateReferences(context);
            }
            else
            {
                SkipRevisionFromBuffer(buffer);
            }
        }

        internal override object GetChanges()
        {
            return value;
        }

        internal override void SkipRevisionFromBuffer(NetBuffer buffer)
        {
            buffer.Index += sizeof(ushort);
        } 
    }
}
