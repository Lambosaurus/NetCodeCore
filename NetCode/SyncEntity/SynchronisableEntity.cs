using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncField;
using NetCode.Util;

namespace NetCode.SyncEntity
{
    internal class SynchronisableEntity : IVersionable
    {
        const int ID_HEADER_LENGTH = sizeof(ushort);
        const int TYPEID_HEADER_LENGTH = sizeof(ushort);
        const int FIELD_COUNT_HEADER_LENGTH = sizeof(byte);
        const int FIELDID_HEADER_LENGTH = sizeof(byte);

        private SyncEntityDescriptor descriptor;
        private SynchronisableField[] fields;

        public bool Changed { get; private set; } = true;
        public uint Revision { get; private set; } = 0;
        public uint EntityID { get; private set; }
        public ushort TypeID { get { return descriptor.TypeID; } }
        
        internal SynchronisableEntity(SyncEntityDescriptor _descriptor, uint entityID)
        {
            descriptor = _descriptor;
            EntityID = entityID;

            fields = descriptor.GenerateFields();
        }

        /// <summary>
        /// Returns the number of bytes required to write this object into the packet.
        /// This returns 0 if no fields have changed.
        /// </summary>
        public int WriteToBufferSize()
        {
            if (!Changed) { return 0; }

            int size = ID_HEADER_LENGTH + FIELD_COUNT_HEADER_LENGTH + TYPEID_HEADER_LENGTH;
            foreach (SynchronisableField field in fields)
            {
                if (field.Changed)
                {
                    size += FIELDID_HEADER_LENGTH + field.WriteToBufferSize();
                }
            }
            return size;
        }


        public static void ReadHeader(byte[] data, ref int index, out uint entityID, out ushort typeID)
        {
            entityID = Primitive.ReadUShort(data, ref index);
            typeID = Primitive.ReadUShort(data, ref index);
        }
        
        public void WriteToBuffer(byte[] data, ref int index, uint revision)
        {
            if (!Changed) { return; }

            byte changed_fields = 0;
            foreach (SynchronisableField field in fields)
            {
                if (field.Changed)
                {
                    changed_fields++;
                }
            }

            Primitive.WriteUShort(data, ref index, (ushort)EntityID);
            Primitive.WriteUShort(data, ref index, descriptor.TypeID);

            Primitive.WriteByte(data, ref index, changed_fields);

            for (byte i = 0; i < descriptor.FieldCount; i++)
            {
                SynchronisableField field = fields[i];
                if (field.Changed)
                {
                    // This MUST be written as a byte.
                    Primitive.WriteByte(data, ref index, (byte)i);
                    field.WriteToBuffer(data, ref index, revision);
                }
            }

            Revision = revision;
            Changed = false;
        }

        public void ReadFromBuffer(byte[] data, ref int index, uint revision)
        {
            byte fieldCount = Primitive.ReadByte(data, ref index);

            for (int i = 0; i < fieldCount; i++)
            {
                //TODO: This is unsafe. The field ID may be out or range, and there
                //      may be insufficient data remaining to call .PullFromBuffer with
                byte fieldID = Primitive.ReadByte(data, ref index);
                fields[fieldID].ReadFromBuffer(data, ref index, revision);
            }

            Revision = revision;
        }

        public void PullFromLocal(object obj)
        {
            for (int i = 0; i < descriptor.FieldCount; i++)
            {
                object value = descriptor.GetField(obj, i);
                fields[i].Update(value);
                if (fields[i].Changed) { Changed = true; }
            }
        }

        public void PushToLocal(object obj)
        {
            for (int i = 0; i < descriptor.FieldCount; i++)
            {
                if (fields[i].Changed)
                {
                    object value = fields[i].GetValue();
                    descriptor.SetField(obj, i, value);
                }
            }
        }
    }
}
