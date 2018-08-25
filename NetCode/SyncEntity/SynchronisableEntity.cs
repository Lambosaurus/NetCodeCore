using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncField;
using NetCode.Util;

namespace NetCode.SyncEntity
{
    internal class SynchronisableEntity
    {
        internal const int HeaderSize = sizeof(ushort) + sizeof(ushort) + sizeof(byte);
        internal const int FieldHeaderSize = sizeof(byte);

        private SyncEntityDescriptor descriptor;
        private SynchronisableField[] fields;
        
        public uint Revision { get; private set; }
        public ushort EntityID { get; private set; }
        public ushort TypeID { get { return descriptor.TypeID; } }
        public bool Synchronised { get; protected set; } = false;
        
        internal SynchronisableEntity(SyncEntityDescriptor _descriptor, ushort entityID, uint revision = 0)
        {
            descriptor = _descriptor;
            EntityID = entityID;

            fields = descriptor.GenerateFields();

            // The revision can be initialised to the creation revision
            // This protects the entity against deletion from an out of date deletion payload
            Revision = revision;
        }

        public static void ReadHeader(byte[] data, ref int index, out ushort entityID, out ushort typeID)
        {
            entityID = Primitive.ReadUShort(data, ref index);
            typeID = Primitive.ReadUShort(data, ref index);
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
                if (field.Revision == revision)
                {
                    return true;
                }
            }
            return false;
        }

        public int WriteRevisionToBufferSize(uint revision)
        {
            int size = HeaderSize;
            foreach (SynchronisableField field in fields)
            {
                if (field.Revision == revision)
                {
                    size += FieldHeaderSize + field.WriteToBufferSize();
                }
            }
            return size;
        }

        public int WriteAllToBufferSize()
        {
            int size = HeaderSize;
            foreach (SynchronisableField field in fields)
            {
                size += FieldHeaderSize + field.WriteToBufferSize();
            }
            return size;
        }
        
        public void WriteRevisionToBuffer(byte[] data, ref int index, uint revision)
        {
            List<byte> updatedFieldIDs = new List<byte>();

            for (byte i = 0; i < fields.Length; i++)
            {
                if (fields[i].Revision == revision)
                {
                    updatedFieldIDs.Add(i);
                }
            }

            Primitive.WriteUShort(data, ref index, EntityID);
            Primitive.WriteUShort(data, ref index, descriptor.TypeID);

            Primitive.WriteByte(data, ref index, (byte)updatedFieldIDs.Count);

            foreach (byte fieldID in updatedFieldIDs)
            {
                SynchronisableField field = fields[fieldID];
                Primitive.WriteByte(data, ref index, fieldID);
                field.Write(data, ref index);
            }
        }

        public void WriteAllToBuffer(byte[] data, ref int index)
        {
            Primitive.WriteUShort(data, ref index, EntityID);
            Primitive.WriteUShort(data, ref index, descriptor.TypeID);
            Primitive.WriteByte(data, ref index, (byte)fields.Length );

            for (byte fieldID = 0; fieldID < fields.Length; fieldID++)
            {
                SynchronisableField field = fields[fieldID];
                Primitive.WriteByte(data, ref index, fieldID);
                field.Write(data, ref index);
            }
        }
        
        public void ReadRevisionFromBuffer(byte[] data, ref int index, SyncContext context)
        {
            byte fieldCount = Primitive.ReadByte(data, ref index);

            for (int i = 0; i < fieldCount; i++)
            {
                //TODO: This is unsafe. The field ID may be out or range, and there
                //      may be insufficient data remaining to call .PullFromBuffer with
                byte fieldID = Primitive.ReadByte(data, ref index);
                SynchronisableField field = fields[fieldID];
                field.ReadChanges(data, ref index, context);
                
                if (!field.Synchronised) { Synchronised = false; }
            }

            if (context.Revision > Revision)
            {
                Revision = context.Revision;
            }
        }
        
        /// <summary>
        /// As per ReadRevisionFromBuffer, but skips the data and keeps the index up to date.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="descriptor"></param>
        internal static void SkipRevisionFromBuffer(byte[] data, ref int index, SyncEntityDescriptor descriptor)
        {
            byte fieldCount = Primitive.ReadByte(data, ref index);

            for (int i = 0; i < fieldCount; i++)
            {
                //TODO: This is unsafe. The field ID may be out or range, and there
                //      may be insufficient data remaining to call .PullFromBuffer with
                byte fieldID = Primitive.ReadByte(data, ref index);
                descriptor.GetStaticField(fieldID).Skip(data, ref index);
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
                    field.Synchronised = true;
                }
            }
            Synchronised = true;
        }
    }
}
