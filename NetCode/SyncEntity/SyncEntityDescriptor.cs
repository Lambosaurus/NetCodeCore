using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;

using NetCode.SyncField;
using NetCode.Util;

namespace NetCode.SyncEntity
{
    internal class SyncEntityDescriptor
    {
        List<SyncFieldDescriptor> fieldDescriptors = new List<SyncFieldDescriptor>();
        Func<object> Constructor;

        public int FieldCount { get; private set; }
        public ushort TypeID { get; private set; }


        // This is a set of fields used in the odd cases where an entity update must be skipped.
        private SynchronisableField[] StaticFields;

        public SyncEntityDescriptor(Type entityType, ushort typeID)
        {
            TypeID = typeID;
            Constructor = DelegateGenerator.GenerateConstructor<object>(entityType);

            AttributeHelper.ForAllFieldsWithAttribute<SynchronisableAttribute>(entityType,
               (fieldInfo, attribute) => {
                   fieldDescriptors.Add(SyncFieldGenerator.GenerateFieldDescriptor(fieldInfo, attribute.Flags));
               });
            AttributeHelper.ForAllPropertiesWithAttribute<SynchronisableAttribute>(entityType,
               (propInfo, attribute) => {
                   fieldDescriptors.Add(SyncFieldGenerator.GenerateFieldDescriptor(propInfo, attribute.Flags));
               });

            FieldCount = fieldDescriptors.Count;
            if (FieldCount >= byte.MaxValue) { throw new NetcodeItemcountException(string.Format("Type {0} contains more than {1} synchronisable fields.", entityType.Name, byte.MaxValue)); }

            StaticFields = GenerateFields();
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

        internal SynchronisableField GetStaticField(int fieldID)
        {
            return StaticFields[fieldID];
        }

        public void SetField(object obj, int fieldID, object value)
        {
            fieldDescriptors[fieldID].Setter(obj, value);
        }

        public object GetField(object obj, int fieldID)
        {
            return fieldDescriptors[fieldID].Getter(obj);
        }

        public object ConstructObject()
        {
            return Constructor.Invoke();
        }

    }
}
