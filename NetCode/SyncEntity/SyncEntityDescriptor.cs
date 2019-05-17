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
        public List<SyncFieldDescriptor> FieldDescriptors { get; private set; } = new List<SyncFieldDescriptor>();
        Func<object> Constructor;

        public int FieldCount { get; private set; }
        public ushort TypeID { get; private set; }


        public SyncEntityDescriptor(Type entityType, ushort typeID)
        {
            TypeID = typeID;
            Constructor = DelegateGenerator.GenerateConstructor<object>(entityType);

            AttributeHelper.ForAllFieldsWithAttribute<SynchronisableAttribute>(entityType,
               (fieldInfo, attribute) => {
                   FieldDescriptors.Add(SyncFieldGenerator.GenerateFieldDescriptor(fieldInfo, attribute.Flags));
               });
            AttributeHelper.ForAllPropertiesWithAttribute<SynchronisableAttribute>(entityType,
               (propInfo, attribute) => {
                   FieldDescriptors.Add(SyncFieldGenerator.GenerateFieldDescriptor(propInfo, attribute.Flags));
               });

            FieldCount = FieldDescriptors.Count;
            if (FieldCount >= byte.MaxValue) { throw new NetcodeItemcountException(string.Format("Type {0} contains more than {1} synchronisable fields.", entityType.Name, byte.MaxValue)); }
        }

        public SynchronisableField[] GenerateFields()
        {
            SynchronisableField[] fields = new SynchronisableField[FieldDescriptors.Count];
            for (int i = 0; i < FieldDescriptors.Count; i++)
            {
                fields[i] = FieldDescriptors[i].Factory.Construct();
            }
            return fields;
        }

        public void SetField(object obj, int fieldID, object value)
        {
            FieldDescriptors[fieldID].Setter(obj, value);
        }

        public object GetField(object obj, int fieldID)
        {
            return FieldDescriptors[fieldID].Getter(obj);
        }

        public void SkipFromBuffer(NetBuffer buffer, int fieldId)
        {
            FieldDescriptors[fieldId].Factory.SkipFromBuffer(buffer);
        }

        public object ConstructObject()
        {
            return Constructor.Invoke();
        }

    }
}
