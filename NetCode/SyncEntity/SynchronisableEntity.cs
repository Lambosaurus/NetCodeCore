using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncPool;
using NetCode.SyncField;
using NetCode.Util;

namespace NetCode.SyncEntity
{
    public class SynchronisableEntity
    {
        internal const int HeaderSize = sizeof(ushort) + sizeof(ushort) + sizeof(byte);
        internal const int FieldHeaderSize = sizeof(byte);
        internal const byte FieldHeaderAll = 0x00;

        private SyncEntityDescriptor descriptor;
        private SynchronisableField[] fields;
        
        public uint Revision { get; private set; }
        public ushort EntityID { get; private set; }
        public ushort TypeID { get { return descriptor.TypeID; } }
        public bool Synchronised { get; protected set; } = false;
        public bool ReferencesPending { get; protected set; } = false;
        
        
        internal SynchronisableEntity(SyncEntityDescriptor _descriptor, ushort entityID, uint revision = 0)
        {
            descriptor = _descriptor;
            EntityID = entityID;

            fields = descriptor.GenerateFields();

            // The revision can be initialised to the creation revision
            // This protects the entity against deletion from an out of date deletion payload
            Revision = revision;
        }

        public static void ReadHeader(NetBuffer buffer, out ushort entityID, out ushort typeID)
        {
            entityID = buffer.ReadUShort();
            typeID = buffer.ReadUShort();
        }

        public bool ContainsRevision(uint revision)
        {
            if (Revision < revision)
            {
                // Can return immediately, because this we cannot have any newer content
                return false;
            }
            else if (Revision == revision)
            {
                return true;
            }

            foreach (SynchronisableField field in fields)
            {
                if (field.ContainsRevision(revision))
                {
                    return true;
                }
            }
            return false;
        }

        public int WriteRevisionToBufferSize(uint revision)
        {
            byte updatedFields = 0;
            int size = HeaderSize;
            foreach (SynchronisableField field in fields)
            {
                if (field.ContainsRevision(revision))
                {
                    updatedFields++;
                    size += field.WriteToBufferSize(revision);
                }
            }
            if (updatedFields != fields.Length)
            {
                // If we cannot write all fields at once, then we need to include field headers.
                size += FieldHeaderSize * updatedFields;
            }
            return size;
        }

        public int WriteAllToBufferSize()
        {
            int size = HeaderSize;
            foreach (SynchronisableField field in fields)
            {
                size += field.WriteToBufferSize();
            }
            return size;
        }
        
        public void WriteRevisionToBuffer(NetBuffer buffer, SyncContext context)
        {
            List<byte> updatedFieldIDs = new List<byte>();

            for (byte i = 0; i < fields.Length; i++)
            {
                if (fields[i].ContainsRevision(context.Revision))
                {
                    updatedFieldIDs.Add(i);
                }
            }

            buffer.WriteUShort(EntityID);
            buffer.WriteUShort(descriptor.TypeID);

            if (updatedFieldIDs.Count == fields.Length)
            {
                buffer.WriteByte(FieldHeaderAll);

                foreach(SynchronisableField field in fields)
                {
                    field.WriteToBuffer(buffer, context);
                }
            }
            else
            {
                buffer.WriteByte((byte)updatedFieldIDs.Count);

                foreach (byte fieldID in updatedFieldIDs)
                {
                    buffer.WriteByte(fieldID);
                    fields[fieldID].WriteToBuffer(buffer, context);
                }
            }
        }

        public void WriteAllToBuffer(NetBuffer buffer)
        {
            buffer.WriteUShort(EntityID);
            buffer.WriteUShort(descriptor.TypeID);
            buffer.WriteByte(FieldHeaderAll);

            foreach (SynchronisableField field in fields) 
            {
                field.WriteToBuffer(buffer);
            }
        }
        
        public void ReadRevisionFromBuffer(NetBuffer buffer, SyncContext context)
        {
            byte fieldCount = buffer.ReadByte();

            if (fieldCount == FieldHeaderAll)
            {
                foreach (SynchronisableField field in fields)
                {
                    field.ReadFromBuffer(buffer, context);
                    Synchronised &= field.Synchronised;
                    ReferencesPending |= field.ReferencesPending;
                }
            }
            else
            {
                for (byte i = 0; i < fieldCount; i++)
                {
                    //TODO: This is unsafe. The field ID may be out or range, and there
                    //      may be insufficient data remaining to call .ReadRevisionFromBuffer with

                    byte fieldID = buffer.ReadByte();
                    SynchronisableField field = fields[fieldID];
                    field.ReadFromBuffer(buffer, context);
                    Synchronised &= field.Synchronised;
                    ReferencesPending |= field.ReferencesPending;
                }
            }

            if (context.Revision > Revision)
            {
                Revision = context.Revision;
            }
        }


        public void UpdateReferences(SyncContext context)
        {
            ReferencesPending = false;
            foreach (SynchronisableField field in fields)
            {
                if (field.ReferencesPending)
                {
                    field.UpdateReferences(context);
                    if (field.ReferencesPending)
                    {
                        ReferencesPending = true;
                    }
                }
                if (!field.Synchronised)
                {
                    Synchronised = false;
                }
            }
        }
        
        /// <summary>
        /// As per ReadRevisionFromBuffer, but skips the data and keeps the index up to date.
        /// </summary>
        internal static void SkipRevisionFromBuffer(NetBuffer buffer, SyncEntityDescriptor descriptor)
        {
            byte fieldCount = buffer.ReadByte();

            if (fieldCount == FieldHeaderAll)
            {
                for (int i = 0; i < descriptor.FieldCount; i++)
                {
                    descriptor.SkipFromBuffer(buffer, i);
                }
            }
            else
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    //TODO: This is unsafe. The field ID may be out or range, and there
                    //      may be insufficient data remaining to call .PullFromBuffer with
                    byte fieldID = buffer.ReadByte();
                    descriptor.SkipFromBuffer(buffer, fieldID);
                }
            }
        }

        public bool TrackChanges(object obj, SyncContext context)
        {
            bool changesFound = false;
            for (int i = 0; i < descriptor.FieldCount; i++)
            {
                object value = descriptor.GetField(obj, i);
                if (fields[i].TrackChanges(value, context))
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

        public void PushChanges(object obj)
        {
            for (int i = 0; i < descriptor.FieldCount; i++)
            {
                SynchronisableField field = fields[i];
                if (!field.Synchronised)
                {
                    object value = field.GetValue();
                    descriptor.SetField(obj, i, value);
                    field.SetSynchonised(true);
                }
            }
            Synchronised = true;
        }
    }
}
