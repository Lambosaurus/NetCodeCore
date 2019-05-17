using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using NetCode.Util;
using NetCode.SyncField.Implementations;

namespace NetCode.SyncField
{
    internal static class SyncFieldGenerator
    {
        private static Dictionary<RuntimeTypeHandle, SyncFieldValueFactory> HalfFieldFactoryLookup = new Dictionary<RuntimeTypeHandle, SyncFieldValueFactory>();
        private static Dictionary<RuntimeTypeHandle, SyncFieldValueFactory> FieldFactoryLookup = new Dictionary<RuntimeTypeHandle, SyncFieldValueFactory>();
        private static SyncFieldValueFactory TimestampFieldFactory;

        static SyncFieldGenerator()
        {
            TimestampFieldFactory = new SyncFieldValueFactory(typeof(SyncFieldTimestamp));
            LoadFieldTypes();
        }

        private static void LoadFieldTypes()
        {
            AttributeHelper.ForAllTypesWithAttribute<EnumerateSyncFieldAttribute>(
                (type, attribute) => { RegisterFieldType(type, attribute.FieldType, attribute.Flags); }
                );
        }

        private static void RegisterFieldType(Type syncFieldType, Type fieldType, SyncFlags syncFlags = SyncFlags.None)
        {
            if (!syncFieldType.IsSubclassOf(typeof(SynchronisableField)))
            {
                throw new NotSupportedException(string.Format(
                    "{0} must inherit from SynchronisableField.",
                    syncFieldType.Name
                    ));
            }
            RuntimeTypeHandle fieldTypeHandle = fieldType.TypeHandle;
            Dictionary<RuntimeTypeHandle, SyncFieldValueFactory> lookup = ((syncFlags & SyncFlags.HalfPrecision) != 0)
                                                               ? HalfFieldFactoryLookup : FieldFactoryLookup;

            if (lookup.ContainsKey(fieldTypeHandle))
            {
                throw new NotSupportedException(string.Format(
                    "A SynchronisableField has already been registered against {0} with flags {1}",
                    fieldType.Name, syncFlags
                    ));
            }
            lookup[fieldTypeHandle] = new SyncFieldValueFactory(syncFieldType); ;
        }

        private static SyncFieldFactory GenerateFieldFactoryByType(Type type, SyncFlags flags)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Get the factory used to create generate the element.
                Type elementType = type.GetGenericArguments()[0];
                SyncFieldFactory elementFactory = GenerateFieldFactoryByType(elementType, flags);
                
                // Generate the generic factory and create an instance
                Type syncFactoryType = typeof(SyncFieldListFactory<>).MakeGenericType(new Type[] { elementType });
                // SyncFieldLists take a SyncFieldFactory as an argument.
                ConstructorInfo constructor = syncFactoryType.GetConstructor(new Type[] { typeof(SyncFieldFactory) });
                return (SyncFieldFactory)constructor.Invoke(new object[] { elementFactory });
                
            }
            else if (type.IsArray)
            {
                // Get the factory used to create generate the element.
                Type elementType = type.GetElementType();
                SyncFieldFactory elementFactory = GenerateFieldFactoryByType(elementType, flags);

                // Generate the generic factory and create an instance
                Type syncFactoryType = typeof(SyncFieldArrayFactory<>).MakeGenericType(new Type[] { elementType });
                // SyncFieldArrays take a SyncFieldFactory as an argument.
                ConstructorInfo constructor = syncFactoryType.GetConstructor(new Type[] { typeof(SyncFieldFactory) });
                return (SyncFieldFactory)constructor.Invoke(new object[] { elementFactory });
            }
            else if ((flags & SyncFlags.Reference) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NotSupportedException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.Reference));
                }
                if ((flags & SyncFlags.Linked) != 0)
                {
                    return new SyncFieldLinkedReferenceFactory(type);
                }
                else
                {
                    return new SyncFieldReferenceFactory(type);
                }
            }
            else if ((flags & SyncFlags.Timestamp) != 0)
            {
                if (type != typeof(long))
                {
                    throw new NotSupportedException(string.Format("{0}.{1} must be used on type long", typeof(SyncFlags).Name, SyncFlags.Timestamp));
                }
                return TimestampFieldFactory;
            }

            RuntimeTypeHandle typeHandle = (type.IsEnum)
                ? typeof(System.Enum).TypeHandle // Enums all inherit from System.Enum, but have distint typehandles otherwise.
                : type.TypeHandle;

            if ((flags & SyncFlags.HalfPrecision) != 0)
            {
                if (HalfFieldFactoryLookup.Keys.Contains(typeHandle))
                {
                    return HalfFieldFactoryLookup[typeHandle];
                }
                else
                {
                    throw new NotSupportedException(string.Format(
                        "No SyncField registered for type {0} with {1}.{2}",
                        type.Name, typeof(SyncFlags).Name, SyncFlags.HalfPrecision
                        ));
                }
            }
            else
            {
                if (FieldFactoryLookup.Keys.Contains(typeHandle))
                {
                    return FieldFactoryLookup[typeHandle];
                }
                else
                {
                    throw new NotSupportedException(string.Format("No SyncField registered for type {0}",type.Name));
                }
            }
        }
        
        internal static SyncFieldDescriptor GenerateFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            return new SyncFieldDescriptor(
                GenerateFieldFactoryByType(fieldInfo.FieldType, syncFlags),
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo)
                );
        }
        
        internal static SyncFieldDescriptor GenerateFieldDescriptor(PropertyInfo propertyInfo, SyncFlags syncFlags)
        {
            return new SyncFieldDescriptor(
                GenerateFieldFactoryByType(propertyInfo.PropertyType, syncFlags),
                DelegateGenerator.GenerateGetter(propertyInfo),
                DelegateGenerator.GenerateSetter(propertyInfo)
                );
        }
    }
}
