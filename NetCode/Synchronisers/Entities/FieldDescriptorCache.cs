using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using NetCode.Util;
using NetCode.Synchronisers.Containers;
using NetCode.Synchronisers.Values;
using NetCode.Synchronisers.References;
using NetCode.Synchronisers.Timestamp;

namespace NetCode.Synchronisers.Entities
{
    internal class FieldDescriptorCache
    {
        private static Dictionary<RuntimeTypeHandle, SynchroniserFactory> HalfFieldFactoryLookup = new Dictionary<RuntimeTypeHandle, SynchroniserFactory>();
        private static Dictionary<RuntimeTypeHandle, SynchroniserFactory> FieldFactoryLookup = new Dictionary<RuntimeTypeHandle, SynchroniserFactory>();
        private static SynchroniserFactory TimestampLongFactory;
        private static SynchroniserFactory TimestampIntFactory;

        EntityDescriptorCache EntityGenerator;
        public FieldDescriptorCache(EntityDescriptorCache entityGenerator)
        {
            EntityGenerator = entityGenerator;
        }

        static FieldDescriptorCache()
        {
            TimestampLongFactory = new SyncValueFactory(typeof(SyncLongTimestamp));
            TimestampIntFactory = new SyncValueFactory(typeof(SyncIntTimestamp));

            AttributeHelper.ForAllTypesWithAttribute<EnumerateSyncValueAttribute>(
                (type, attribute) => { RegisterFieldType(type, attribute.FieldType, attribute.Flags); }
                );
        }

        private static void RegisterFieldType(Type syncFieldType, Type fieldType, SyncFlags syncFlags = SyncFlags.None)
        {
            if (!syncFieldType.IsSubclassOf(typeof(Synchroniser)))
            {
                throw new NotSupportedException(string.Format(
                    "{0} must inherit from SynchronisableField.",
                    syncFieldType.Name
                    ));
            }
            RuntimeTypeHandle fieldTypeHandle = fieldType.TypeHandle;
            Dictionary<RuntimeTypeHandle, SynchroniserFactory> lookup = ((syncFlags & SyncFlags.HalfPrecision) != 0)
                                                               ? HalfFieldFactoryLookup : FieldFactoryLookup;

            if (lookup.ContainsKey(fieldTypeHandle))
            {
                throw new NotSupportedException(string.Format(
                    "A SynchronisableField has already been registered against {0} with flags {1}",
                    fieldType.Name, syncFlags
                    ));
            }
            lookup[fieldTypeHandle] = new SyncValueFactory(syncFieldType); ;
        }

        private SynchroniserFactory GenerateFieldFactoryByType(Type type, SyncFlags flags)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Recursively get the factory used to create generate the element.
                Type elementType = type.GetGenericArguments()[0];
                SynchroniserFactory elementFactory = GenerateFieldFactoryByType(elementType, flags);
                
                // Generate the generic factory and create an instance
                Type syncFactoryType = typeof(SyncContainerListFactory<>).MakeGenericType(new Type[] { elementType });
                // SyncFieldLists take a SyncFieldFactory as an argument.
                ConstructorInfo constructor = syncFactoryType.GetConstructor(new Type[] { typeof(SynchroniserFactory), typeof(SyncFlags) });
                return (SynchroniserFactory)constructor.Invoke(new object[] { elementFactory, flags });
            }
            else if (type.IsArray)
            {
                // Recursively get the factory used to create generate the element.
                Type elementType = type.GetElementType();
                SynchroniserFactory elementFactory = GenerateFieldFactoryByType(elementType, flags);

                // Generate the generic factory and create an instance
                Type syncFactoryType = typeof(SyncContainerArrayFactory<>).MakeGenericType(new Type[] { elementType });
                // SyncFieldArrays take a SyncFieldFactory as an argument.
                ConstructorInfo constructor = syncFactoryType.GetConstructor(new Type[] { typeof(SynchroniserFactory), typeof(SyncFlags) });
                return (SynchroniserFactory)constructor.Invoke(new object[] { elementFactory, flags });
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
                return new SyncReferenceFactory(type, flags);
            }
            else if ((flags & SyncFlags.Timestamp) != 0)
            {
                if (type == typeof(long))
                {
                    return TimestampLongFactory;
                }
                else if (type == typeof(int))
                {
                    return TimestampIntFactory;
                }
                throw new NotSupportedException(string.Format("{0}.{1} must be used on type long or int", typeof(SyncFlags).Name, SyncFlags.Timestamp));
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
