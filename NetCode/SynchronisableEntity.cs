using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NetCode
{
    public class SyncHandle
    {
        public SynchronisableEntity sync;
        public Object obj;
    }

    public class SynchronisableEntity
    {
        const int UUID_HEADER_LENGTH = sizeof(uint);
        const int FIELD_COUNT_HEADER_LENGTH = sizeof(byte);
        const int FIELD_ID_HEADER_LENGTH = sizeof(byte);

        private SynchronisableEntityDescriptor descriptor;
        private SynchronisableField[] fields;

        public byte Changed { get; private set; } = 0;
        public uint Uuid { get; private set; }


        public SynchronisableEntity(SynchronisableEntityDescriptor _descriptor, uint id)
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
            if (Changed == 0) { return 0; }

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
            if (Changed == 0) { return; }

            PrimitiveSerialiser.Write(data, ref index, Uuid);
            PrimitiveSerialiser.Write(data, ref index, Changed);

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

            Changed = 0;
        }

        public void UpdateFromLocal(object obj)
        {
            for (int i = 0; i < descriptor.FieldCount; i++)
            {
                object value = descriptor.GetValue(obj, i);
                fields[i].Update(value);
                if (fields[i].Changed) { Changed++; }
            }
        }
    }

    public class SynchronisableEntityDescriptor
    {
        private class SynchronisableFieldDescriptor
        {
            public FieldInfo info;
            public SyncFlags flags;
            public int typeindex;
        }

        List<SynchronisableFieldDescriptor> fieldDescriptors = new List<SynchronisableFieldDescriptor>();

        private Func<object> constructor;

        public int FieldCount { get; private set; }

        SynchronisableFieldGenerator field_generator;

        public SynchronisableEntityDescriptor(SynchronisableFieldGenerator _field_generator, Type sync_type)
        {
            field_generator = _field_generator;
            constructor = DelegateGenerator.GenerateConstructor(sync_type);
            
            //constructorinfo = sync_type.GetConstructor(new Type[0]);
            //if (constructorinfo == null)
            //{
            //    throw new NotSupportedException(string.Format("Type {0} must provide a constructor with zero arguments.", sync_type.Name));
            //}

            foreach (FieldInfo info in sync_type.GetFields())
            {
                foreach (object attribute in info.GetCustomAttributes(true))
                {
                    if (attribute is SynchronisableAttribute)
                    {
                        SyncFlags flags = ((SynchronisableAttribute)attribute).flags;
                        SynchronisableFieldDescriptor descriptor = new SynchronisableFieldDescriptor
                        {
                            info = info,
                            flags = flags,
                            typeindex = field_generator.LookupFieldIndex( info.FieldType, flags )
                        };

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
                fields[i] = field_generator.GenerateField(fieldDescriptors[i].typeindex);
            }
            return fields;
        }

        
        public void SetVariable(object obj, int index, object value)
        {
            fieldDescriptors[index].info.SetValue(obj, value);
        }

        public object GetValue(object obj, int index)
        {
            return fieldDescriptors[index].info.GetValue(obj);
        }
        

        public object ConstructObject()
        {
            return constructor();
        }

    }
}
