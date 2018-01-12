using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NetCode
{
    public class SyncHandle
    {
        internal SyncEntity sync;
        public Object obj { get; internal set; }
    }

    internal class SyncEntity
    {
        const int UUID_HEADER_LENGTH = sizeof(uint);
        const int FIELD_COUNT_HEADER_LENGTH = sizeof(byte);
        const int FIELD_ID_HEADER_LENGTH = sizeof(byte);

        private SyncEntityDescriptor descriptor;
        private SynchronisableField[] fields;

        public bool Changed { get; private set; } = true;
        public uint Uuid { get; private set; }


        internal SyncEntity(SyncEntityDescriptor _descriptor, uint id)
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

            int size = UUID_HEADER_LENGTH + FIELD_COUNT_HEADER_LENGTH;
            foreach (SynchronisableField field in fields)
            {
                if (field.Changed)
                {
                    size += FIELD_ID_HEADER_LENGTH + field.WriteSize();
                }
            }
            return size;
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

            PrimitiveSerialiser.Write(data, ref index, Uuid);
            PrimitiveSerialiser.Write(data, ref index, changed_fields);

            for (byte i = 0; i < descriptor.FieldCount; i++)
            {
                SynchronisableField field = fields[i];
                if (field.Changed)
                {
                    // This MUST be written as a byte.
                    PrimitiveSerialiser.Write(data, ref index, (byte)i);
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

    internal class SyncEntityDescriptor
    {
        List<SyncFieldDescriptor> fieldDescriptors = new List<SyncFieldDescriptor>();
        Func<object> constructor;

        public int FieldCount { get; private set; }
        

        public SyncEntityDescriptor(SyncFieldgenerator fieldGenerator, Type entityType)
        {
            constructor = DelegateGenerator.GenerateConstructor(entityType);
            
            foreach (FieldInfo info in entityType.GetFields())
            {
                foreach (object attribute in info.GetCustomAttributes(true))
                {
                    if (attribute is SynchronisableAttribute)
                    {
                        SyncFlags flags = ((SynchronisableAttribute)attribute).flags;
                        SyncFieldDescriptor descriptor = fieldGenerator.GenerateFieldDescriptor(info, flags);

                        fieldDescriptors.Add(descriptor);
                    }
                }
            }
            FieldCount = fieldDescriptors.Count;
        }

        public SynchronisableField[] GenerateFields()
        {
            SynchronisableField[] fields = new SynchronisableField[fieldDescriptors.Count];
            for (int i = 0; i < fieldDescriptors.Count; i++)
            {
                fields[i] = fieldDescriptors[i].GenerateField();
            }
            return fields;
        }

        
        public void SetField(object obj, int index, object value)
        {
            fieldDescriptors[index].Setter(obj, value);
        }

        public object GetField(object obj, int index)
        {
            return fieldDescriptors[index].Getter(obj);
        }
        
        public object ConstructObject()
        {
            return constructor.Invoke();
        }

    }
}
