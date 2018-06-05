using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using NetCode.SyncField;

namespace NetCode.SyncEntity
{
    internal class SyncEntityDescriptor
    {
        List<SyncFieldDescriptor> fieldDescriptors = new List<SyncFieldDescriptor>();
        Func<object> Constructor;

        public int FieldCount { get; private set; }
        public ushort TypeID { get; private set; }

        public SyncEntityDescriptor(SyncFieldGenerator fieldGenerator, Type entityType, ushort typeID)
        {
            TypeID = typeID;
            Constructor = DelegateGenerator.GenerateConstructor(entityType);

            foreach (FieldInfo info in entityType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (object attribute in info.GetCustomAttributes(true))
                {
                    if (attribute is SynchronisableAttribute)
                    {
                        SyncFlags flags = ((SynchronisableAttribute)attribute).Flags;
                        SyncFieldDescriptor descriptor = fieldGenerator.GenerateFieldDescriptor(info, flags);

                        fieldDescriptors.Add(descriptor);
                    }
                }
            }
            FieldCount = fieldDescriptors.Count;
            if (FieldCount >= byte.MaxValue) { throw new NetcodeOverloadedException(string.Format("There may not be more than {0} synchronisable fields per type.", byte.MaxValue)); }
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
            return Constructor.Invoke();
        }

    }
}
