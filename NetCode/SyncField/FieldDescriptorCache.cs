using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using NetCode.Util;
using NetCode.SyncField.Implementations;
using NetCode.SyncField.Entities;

namespace NetCode.SyncField
{
    internal class FieldDescriptorCache
    {
        private static Dictionary<RuntimeTypeHandle, SyncFieldFactory> HalfFieldFactoryLookup = new Dictionary<RuntimeTypeHandle, SyncFieldFactory>();
        private static Dictionary<RuntimeTypeHandle, SyncFieldFactory> FieldFactoryLookup = new Dictionary<RuntimeTypeHandle, SyncFieldFactory>();
        private static SyncFieldFactory TimestampFieldFactory;

        EntityDescriptorCache EntityGenerator;
        public FieldDescriptorCache(EntityDescriptorCache entityGenerator)
        {
            EntityGenerator = entityGenerator;
        }

        static FieldDescriptorCache()
        {
            TimestampFieldFactory = new SynchronisableValue.Factory(typeof(SyncFieldTimestamp));

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
            Dictionary<RuntimeTypeHandle, SyncFieldFactory> lookup = ((syncFlags & SyncFlags.HalfPrecision) != 0)
                                                               ? HalfFieldFactoryLookup : FieldFactoryLookup;

            if (lookup.ContainsKey(fieldTypeHandle))
            {
                throw new NotSupportedException(string.Format(
                    "A SynchronisableField has already been registered against {0} with flags {1}",
                    fieldType.Name, syncFlags
                    ));
            }
            lookup[fieldTypeHandle] = new SynchronisableValue.Factory(syncFieldType); ;
        }

        private SyncFieldFactory GenerateFieldFactoryByType(Type type, SyncFlags flags)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Recursively get the factory used to create generate the element.
                Type elementType = type.GetGenericArguments()[0];
                SyncFieldFactory elementFactory = GenerateFieldFactoryByType(elementType, flags);
                
                // Generate the generic factory and create an instance
                Type syncFactoryType = typeof(SyncFieldListFactory<>).MakeGenericType(new Type[] { elementType });
                // SyncFieldLists take a SyncFieldFactory as an argument.
                ConstructorInfo constructor = syncFactoryType.GetConstructor(new Type[] { typeof(SyncFieldFactory), typeof(SyncFlags) });
                return (SyncFieldFactory)constructor.Invoke(new object[] { elementFactory, flags });
            }
            else if (type.IsArray)
            {
                // Recursively get the factory used to create generate the element.
                Type elementType = type.GetElementType();
                SyncFieldFactory elementFactory = GenerateFieldFactoryByType(elementType, flags);

                // Generate the generic factory and create an instance
                Type syncFactoryType = typeof(SyncFieldArrayFactory<>).MakeGenericType(new Type[] { elementType });
                // SyncFieldArrays take a SyncFieldFactory as an argument.
                ConstructorInfo constructor = syncFactoryType.GetConstructor(new Type[] { typeof(SyncFieldFactory), typeof(SyncFlags) });
                return (SyncFieldFactory)constructor.Invoke(new object[] { elementFactory, flags });
            }
            else if ((flags & SyncFlags.NestedEntity) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NotSupportedException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.NestedEntity));
                }
                return EntityGenerator.GetEntityFactory(type.TypeHandle);
            }
            else if ((flags & SyncFlags.Reference) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NotSupportedException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.Reference));
                }
                return new SyncFieldReferenceFactory(type, flags);
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
        
        internal FieldDescriptor GetFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            return new FieldDescriptor(
                GenerateFieldFactoryByType(fieldInfo.FieldType, syncFlags),
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo)
                );
        }
        
        internal FieldDescriptor GetFieldDescriptor(PropertyInfo propertyInfo, SyncFlags syncFlags)
        {
            return new FieldDescriptor(
                GenerateFieldFactoryByType(propertyInfo.PropertyType, syncFlags),
                DelegateGenerator.GenerateGetter(propertyInfo),
                DelegateGenerator.GenerateSetter(propertyInfo)
                );
        }
    }
}
