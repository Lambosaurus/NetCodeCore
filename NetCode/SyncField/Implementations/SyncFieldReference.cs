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

        private SyncFieldDescriptor Descriptor;
        
        internal override void Initialise(SyncFieldDescriptor descriptor, byte elementDepth)
        {
            Descriptor = descriptor;
        }

        public sealed override bool TrackChanges(object newValue, SyncContext context)
        {
            if (newValue != value)
            {
                value = newValue;
                if (value == null)
                {
                    EntityID = SyncHandle.NullEntityID;
                }
                else
                {
                    SyncHandle handle = context.GetHandleByObject(value);

                    // If no handle is found, then the supplie object is not in the syncpool.
                    //TODO: Here we could optionally add the object to the pool for laughs.
                    EntityID = (handle != null) ? handle.EntityID : SyncHandle.NullEntityID;
                }

                Synchronised = false;
                Revision = context.Revision;
                return true;
            }
            return false;
        }

        public sealed override void UpdateReferences(SyncContext context)
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
        public sealed override int WriteToBufferSize()
        {
            return sizeof(ushort);
        }

        public sealed override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteUShort(EntityID);
        }

        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                EntityID = buffer.ReadUShort();
                Revision = context.Revision;

                // In the event UpdateReferences fails, we should still fall back to null.
                value = null;
                Synchronised = false;

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
            buffer.Index += sizeof(ushort);
        } 
    }
}
