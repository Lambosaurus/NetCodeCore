using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncField;

namespace NetCode.SyncEntity
{
    internal class SynchronisableEntity : IBufferable
    {
        const int ID_HEADER_LENGTH = sizeof(ushort);
        const int TYPEID_HEADER_LENGTH = sizeof(ushort);
        const int FIELD_COUNT_HEADER_LENGTH = sizeof(byte);
        const int FIELDID_HEADER_LENGTH = sizeof(byte);

        private SyncEntityDescriptor descriptor;
        private SynchronisableField[] fields;

        public bool Changed { get; private set; } = true;
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
        public int WriteSize()
        {
            if (!Changed) { return 0; }

            int size = ID_HEADER_LENGTH + FIELD_COUNT_HEADER_LENGTH + TYPEID_HEADER_LENGTH;
            foreach (SynchronisableField field in fields)
            {
                if (field.Changed)
                {
                    size += FIELDID_HEADER_LENGTH + field.WriteSize();
                }
            }
            return size;
        }


        public static void ReadHeader(byte[] data, ref int index, out uint entityID, out ushort typeID)
        {
            entityID = PrimitiveSerialiser.ReadUShort(data, ref index);
            typeID = PrimitiveSerialiser.ReadUShort(data, ref index);
        }


        /// <summary>
        /// Writes the packet to the given packet.
        /// This also clears the changed flag.
        /// </summary>
        /// <param name="data">The packet to write to</param>
        /// <param name="index">The index to begin writing to</param>
        public void WriteToBuffer(byte[] data, ref int index, uint packet_id)
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

            PrimitiveSerialiser.WriteUShort(data, ref index, (ushort)EntityID);
            PrimitiveSerialiser.WriteUShort(data, ref index, descriptor.TypeID);

            PrimitiveSerialiser.WriteByte(data, ref index, changed_fields);

            for (byte i = 0; i < descriptor.FieldCount; i++)
            {
                SynchronisableField field = fields[i];
                if (field.Changed)
                {
                    // This MUST be written as a byte.
                    PrimitiveSerialiser.WriteByte(data, ref index, (byte)i);
                    field.WriteToBuffer(data, ref index, packet_id);
                }
            }

            Changed = false;
        }

        public void ReadFromBuffer(byte[] data, ref int index, uint packetID)
        {
            byte fieldCount = PrimitiveSerialiser.ReadByte(data, ref index);

            for (int i = 0; i < fieldCount; i++)
            {
                byte fieldID = PrimitiveSerialiser.ReadByte(data, ref index);
                fields[fieldID].ReadFromBuffer(data, ref index, packetID);
            }
        }

        public void UpdateFromLocal(object obj)
        {
            for (int i = 0; i < descriptor.FieldCount; i++)
            {
                object value = descriptor.GetField(obj, i);
                fields[i].Update(value);
                if (fields[i].Changed) { Changed = true; }
            }
        }

        public void UpdateToLocal(object obj)
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
