using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using NetCode.SyncField;

namespace NetCode.SyncEntity
{
    internal class SynchronisableEntity
    {
        const int UUID_HEADER_LENGTH = sizeof(ushort);
        const int TYPE_HEADER_LENGTH = sizeof(ushort);
        const int FIELD_COUNT_HEADER_LENGTH = sizeof(byte);
        const int FIELD_ID_HEADER_LENGTH = sizeof(byte);

        private SyncEntityDescriptor descriptor;
        private SynchronisableField[] fields;

        public bool Changed { get; private set; } = true;
        public uint Uuid { get; private set; }


        internal SynchronisableEntity(SyncEntityDescriptor _descriptor, uint id)
        {
            descriptor = _descriptor;
            Uuid = id;

            fields = descriptor.GenerateFields();
        }

        /// <summary>
        /// Returns the number of bytes required to write this object into the packet.
        /// This returns 0 if no fields have changed.
        /// </summary>
        public int WriteSize()
        {
            if (!Changed) { return 0; }

            int size = UUID_HEADER_LENGTH + FIELD_COUNT_HEADER_LENGTH + TYPE_HEADER_LENGTH;
            foreach (SynchronisableField field in fields)
            {
                if (field.Changed)
                {
                    size += FIELD_ID_HEADER_LENGTH + field.WriteSize();
                }
            }
            return size;
        }


        public static void ReadHeader(byte[] data, ref int index, out uint Uuid, out ushort TypeID)
        {
            Uuid = PrimitiveSerialiser.ReadUShort(data, ref index);
            TypeID = PrimitiveSerialiser.ReadUShort(data, ref index);
        }


        /// <summary>
        /// Writes the packet to the given packet.
        /// This also clears the changed flag.
        /// </summary>
        /// <param name="data">The packet to write to</param>
        /// <param name="index">The index to begin writing to</param>
        public void WriteToPacket(byte[] data, ref int index, uint packet_id)
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

            PrimitiveSerialiser.WriteUShort(data, ref index, (ushort)Uuid);
            PrimitiveSerialiser.WriteUShort(data, ref index, descriptor.TypeID);
            PrimitiveSerialiser.WriteByte(data, ref index, changed_fields);

            for (byte i = 0; i < descriptor.FieldCount; i++)
            {
                SynchronisableField field = fields[i];
                if (field.Changed)
                {
                    // This MUST be written as a byte.
                    PrimitiveSerialiser.WriteByte(data, ref index, (byte)i);
                    field.WriteToPacket(data, ref index, packet_id);
                }
            }

            Changed = false;
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
    }
}
