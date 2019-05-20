using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;
using NetCode.Util;

namespace NetCode.SyncField.Entities
{
    internal class SyncFieldEntity : SynchronisableField
    {
        protected EntityDescriptor Descriptor;        
        protected SynchronisableField[] Fields;

        public object Value { get; private set; }

        public ushort TypeID { get { return Descriptor.TypeID; } }

        public SyncFieldEntity(EntityDescriptor descriptor, object obj, uint revision)
        {
            Descriptor = descriptor;
            Revision = revision;
            Value = obj;

            Fields = new SynchronisableField[Descriptor.Fields.Length];
            for (int i = 0; i < Fields.Length; i++)
            {
                Fields[i] = Descriptor.Fields[i].Factory.Construct();
            }
        }

        public sealed override object GetValue()
        {
            for (int i = 0; i < Fields.Length; i++)
            {
                Descriptor.Fields[i].Setter(Value, Fields[i].GetValue());
            }
            return Value;
        }

        public sealed override bool ContainsRevision(uint revision)
        {
            if (Revision < revision)
            {
                return false;
            }
            else if (Revision == revision)
            {
                return true;
            }

            foreach (SynchronisableField field in Fields)
            {
                if (field.ContainsRevision(revision))
                {
                    return true;
                }
            }
            return false;
        }

        public sealed override void SetSynchonised(bool sync)
        {
            Synchronised = sync;
            foreach (SynchronisableField field in Fields)
            {
                if (field.Synchronised != sync)
                {
                    field.SetSynchonised(sync);
                }
            }
        }

        public sealed override void UpdateReferences(SyncContext context)
        {
            ReferencesPending = false;
            foreach ( SynchronisableField field in Fields)
            {
                if (field.ReferencesPending)
                {
                    field.UpdateReferences(context);
                    ReferencesPending |= field.ReferencesPending;
                }
            }
        }

        public override bool TrackChanges(object newValue, SyncContext context)
        {
            bool changesFound = false;
            Value = newValue;

            for (int i = 0; i < Fields.Length; i++)
            {
                object field = Descriptor.Fields[i].Getter(Value);
                if (Fields[i].TrackChanges(field, context))
                {
                    changesFound = true;
                }
            }

            if (changesFound)
            {
                Revision = context.Revision;
            }
            return changesFound;
        }

        public sealed override void WriteToBuffer(NetBuffer buffer, SyncContext context)
        {
            List<byte> updatedFields = new List<byte>();

            for (byte i = 0; i < Fields.Length; i++)
            {
                if (Fields[i].ContainsRevision(context.Revision))
                {
                    updatedFields.Add(i);
                }
            }

            bool skipHeaders = updatedFields.Count == Fields.Length;
            buffer.WriteByte((byte)updatedFields.Count);

            if (skipHeaders)
            {
                foreach (SynchronisableField field in Fields)
                {
                    field.WriteToBuffer(buffer, context);
                }
            }
            else
            {
                foreach (byte index in updatedFields)
                {
                    buffer.WriteByte(index);
                    Fields[index].WriteToBuffer(buffer, context);
                }
            }
        }

        public sealed override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteByte((byte)Fields.Length);
            foreach ( SynchronisableField field in Fields )
            {
                field.WriteToBuffer(buffer);
            }
        }

        public sealed override int WriteToBufferSize(uint revision)
        {
            int size = sizeof(byte);

            byte updatedFields = 0;
            foreach (SynchronisableField field in Fields)
            {
                if (field.ContainsRevision(revision))
                {
                    updatedFields++;
                    size += field.WriteToBufferSize(revision);
                }
            }
            if (updatedFields != Fields.Length)
            {
                // If we cannot write all fields at once, then we need to include field headers.
                size += sizeof(byte) * updatedFields;
            }
            return size;
        }

        public sealed override int WriteToBufferSize()
        {
            int size = sizeof(byte);
            foreach (SynchronisableField field in Fields)
            {
                size += field.WriteToBufferSize();
            }
            return size;
        }

        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if (context.Revision > Revision)
            {
                Revision = context.Revision;
            }

            int updated = buffer.ReadByte();
            bool skipHeaders = updated == Fields.Length;

            for (int i = 0; i < updated; i++)
            {
                int index = (skipHeaders) ? i : buffer.ReadByte();
                SynchronisableField field = Fields[index];
                field.ReadFromBuffer(buffer, context);
                Synchronised &= field.Synchronised;
                ReferencesPending |= field.ReferencesPending;
            }
        }

        public sealed override void SkipFromBuffer(NetBuffer buffer)
        {
            int updated = buffer.ReadByte();
            bool skipHeaders = updated == Fields.Length;

            for (int i = 0; i < updated; i++)
            {
                int index = (skipHeaders) ? i : buffer.ReadByte();
                Descriptor.Fields[index].Factory.SkipFromBuffer(buffer);
            }
        }
    }

    internal class SyncFieldEntityFactory : SyncFieldFactory
    {
        public EntityDescriptor Descriptor { get; private set; }

        public SyncFieldEntityFactory(EntityDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public override SynchronisableField Construct()
        {
            return new SyncFieldEntity(Descriptor, Descriptor.Constructor.Invoke(), 0);
        }

        public SyncFieldEntity ConstructNewEntity(uint revision)
        {
            return new SyncFieldEntity(Descriptor, Descriptor.Constructor.Invoke(), revision);
        }

        public SyncFieldEntity ConstructForExisting(object obj)
        {
            return new SyncFieldEntity(Descriptor, obj, 0);
        }
    }
}
