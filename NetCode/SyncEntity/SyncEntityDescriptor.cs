﻿using System;
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

        private const BindingFlags FIELD_SEARCH_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        
        // This is a set of fields used in the odd cases where an entity update must be skipped.
        private SynchronisableField[] StaticFields;

        public SyncEntityDescriptor(SyncFieldGenerator fieldGenerator, Type entityType, ushort typeID)
        {
            TypeID = typeID;
            Constructor = DelegateGenerator.GenerateConstructor(entityType);

            foreach (FieldInfo fieldInfo in entityType.GetFields(FIELD_SEARCH_FLAGS))
            {
                foreach (object attribute in fieldInfo.GetCustomAttributes(true).Where( attr => attr is SynchronisableAttribute ))
                {
                    SyncFlags flags = ((SynchronisableAttribute)attribute).Flags;
                    SyncFieldDescriptor descriptor = fieldGenerator.GenerateFieldDescriptor(fieldInfo, flags);
                    fieldDescriptors.Add(descriptor);
                }
            }

            foreach (PropertyInfo propertyInfo in entityType.GetProperties(FIELD_SEARCH_FLAGS))
            {
                foreach (object attribute in propertyInfo.GetCustomAttributes(true).Where(attr => attr is SynchronisableAttribute))
                {
                    SyncFlags flags = ((SynchronisableAttribute)attribute).Flags;
                    SyncFieldDescriptor descriptor = fieldGenerator.GenerateFieldDescriptor(propertyInfo, flags);
                    fieldDescriptors.Add(descriptor);
                }
            }

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
