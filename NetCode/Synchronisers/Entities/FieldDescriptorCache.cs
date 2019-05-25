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
        private SynchroniserFactory DynamicEntityFactory;

        EntityDescriptorCache EntityCache;
        public FieldDescriptorCache(EntityDescriptorCache entityCache)
        {
            EntityCache = entityCache;
            DynamicEntityFactory = new SyncDynamicEntityFactory(entityCache);
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
            if (!syncFieldType.IsSubclassOf(typeof(SynchronisableValue)))
            {
                throw new NetcodeGenerationException(string.Format(
                    "{0} must inherit from {1}.",
                    syncFieldType.FullName, typeof(SynchronisableValue).Name
                    ));
            }
            RuntimeTypeHandle fieldTypeHandle = fieldType.TypeHandle;
            Dictionary<RuntimeTypeHandle, SynchroniserFactory> lookup = ((syncFlags & SyncFlags.HalfPrecision) != 0)
                                                               ? HalfFieldFactoryLookup : FieldFactoryLookup;

            if (lookup.ContainsKey(fieldTypeHandle))
            {
                throw new NetcodeGenerationException(string.Format(
                    "A {0} has already been registered against {1} with flags {2}",
                    typeof(SynchronisableValue).Name, fieldType.FullName, syncFlags
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
            else if ((flags & SyncFlags.Entity) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NetcodeGenerationException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.Entity));
                }
                if ((flags & SyncFlags.Dynamic) != 0)
                {
                    return DynamicEntityFactory;
                }
                return EntityCache.GetEntityFactory(type.TypeHandle);
            }
            else if ((flags & SyncFlags.Reference) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NetcodeGenerationException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.Reference));
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
                throw new NetcodeGenerationException(string.Format("{0}.{1} must be used on type long or int", typeof(SyncFlags).Name, SyncFlags.Timestamp));
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
                    throw new NetcodeGenerationException(string.Format(
                        "No SyncField registered for type {0} with {1}.{2}",
                        type.FullName, typeof(SyncFlags).Name, SyncFlags.HalfPrecision
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
                    throw new NetcodeGenerationException(string.Format("No {0} registered for type {1}",typeof(SynchronisableValue).Name, type.FullName));
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
