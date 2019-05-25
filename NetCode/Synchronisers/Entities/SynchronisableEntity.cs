using System;
using System.Collections.Generic;
using System.Linq;


namespace NetCode.Synchronisers.Entities
{
    internal class SynchronisableEntity : Synchroniser
    {
        protected EntityDescriptor Descriptor;        
        protected Synchroniser[] Fields;

        public object Value { get; private set; }

        public ushort TypeID { get { return Descriptor.TypeID; } }

        public SynchronisableEntity(EntityDescriptor descriptor) : this(descriptor, null, 0) { }
        public SynchronisableEntity(EntityDescriptor descriptor, object obj, uint revision)
        {
            Descriptor = descriptor;
            Revision = revision;
            Value = obj;

            Fields = new Synchroniser[Descriptor.Fields.Length];
            for (int i = 0; i < Fields.Length; i++)
            {
                Fields[i] = Descriptor.Fields[i].Factory.Construct();
            }
        }

        public bool TypeMatches( RuntimeTypeHandle type )
        {
            return Descriptor.EntityType.TypeHandle.Equals(type);
        }

        public sealed override object GetValue()
        {
            if (Value == null)
            {
                Value = Descriptor.Constructor.Invoke();
            }

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

            foreach (Synchroniser field in Fields)
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
            foreach (Synchroniser field in Fields)
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
            foreach ( Synchroniser field in Fields)
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
            if (newValue != Value)
            {
                // Avoid the typecheck if the object hasnt changed.

                if (!TypeMatches(newValue.GetType().TypeHandle))
                {
                    throw new NetcodeUnexpectedEntityException(
                        string.Format("Object of type {0} was assigned when type {1} was expected. Consider using {2}.{3} if inheritance is required for this entity.",
                        newValue.GetType().FullName, Descriptor.EntityType.FullName, typeof(SyncFlags).Name, SyncFlags.Dynamic
                        ));
                }
                Value = newValue;
            }

            bool changesFound = false;
            

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
                foreach (Synchroniser field in Fields)
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
            foreach ( Synchroniser field in Fields )
            {
                field.WriteToBuffer(buffer);
            }
        }

        public sealed override int WriteToBufferSize(uint revision)
        {
            int size = sizeof(byte);

            byte updatedFields = 0;
            foreach (Synchroniser field in Fields)
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
            foreach (Synchroniser field in Fields)
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
                Synchroniser field = Fields[index];
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
}
