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
        private static Dictionary<RuntimeTypeHandle, Func<SynchronisableField>> HalfConstructorLookups = new Dictionary<RuntimeTypeHandle, Func<SynchronisableField>>();
        private static Dictionary<RuntimeTypeHandle, Func<SynchronisableField>> ConstructorLookups = new Dictionary<RuntimeTypeHandle, Func<SynchronisableField>>();
        private static Func<SynchronisableField> TimestampFieldConstructor;
        private static Func<SynchronisableField> ReferenceFieldConstructor;

        static SyncFieldGenerator()
        {
            TimestampFieldConstructor = DelegateGenerator.GenerateConstructor<SynchronisableField>(typeof(SynchronisableTimestamp));
            ReferenceFieldConstructor = DelegateGenerator.GenerateConstructor<SynchronisableField>(typeof(SynchronisableReference));
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

            Func<SynchronisableField> constructor = DelegateGenerator.GenerateConstructor<SynchronisableField>(syncFieldType);

            Dictionary<RuntimeTypeHandle, Func<SynchronisableField>> lookup = ((syncFlags & SyncFlags.HalfPrecision) != 0)
                                                               ? HalfConstructorLookups : ConstructorLookups;

            if (lookup.ContainsKey(fieldTypeHandle))
            {
                throw new NotSupportedException(string.Format(
                    "A SynchronisableField has already been registered against {0} with flags {1}",
                    fieldType.Name, syncFlags
                    ));
            }

            lookup[fieldTypeHandle] = constructor;
        }

        // These constructors are cashed because these may be run into many times if many Lists and Arrays are used.
        // TODO: there may still be many of these constructors if lots of complex data structures are used.
        //       The memory use should be monitored.
        private static Dictionary<RuntimeTypeHandle, Func<SynchronisableField>> CachedConstructors = new Dictionary<RuntimeTypeHandle, Func<SynchronisableField>>();
        private static Func<SynchronisableField> GenerateAndCacheConstructor(Type type)
        {
            if (CachedConstructors.ContainsKey(type.TypeHandle))
            {
                return CachedConstructors[type.TypeHandle];
            }
            Func<SynchronisableField> constructor = DelegateGenerator.GenerateConstructor<SynchronisableField>(type);
            CachedConstructors[type.TypeHandle] = constructor;
            return constructor;
        }

        private static SyncFieldDescriptor GenerateFieldDescriptorByType(Type type, SyncFlags flags)
        {
            RuntimeTypeHandle typeHandle;

            if (type.IsEnum)
            {
                // Each enum is its own type derived from System.Enum
                typeHandle = typeof(System.Enum).TypeHandle;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = type.GetGenericArguments()[0];
                Type syncListType = typeof(SynchronisableList<>).MakeGenericType(new Type[] { elementType });
                SyncFieldDescriptor elementcontent = GenerateFieldDescriptorByType(elementType, flags);
                elementcontent.InsertParentConstructor(GenerateAndCacheConstructor(syncListType));
                return elementcontent;
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                Type syncListType = typeof(SynchronisableArray<>).MakeGenericType(new Type[] { elementType });
                SyncFieldDescriptor elementcontent = GenerateFieldDescriptorByType(elementType, flags);
                elementcontent.InsertParentConstructor(GenerateAndCacheConstructor(syncListType));
                return elementcontent;
            }
            else if ((flags & SyncFlags.Reference) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NotSupportedException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.Reference));
                }
                return new SyncFieldDescriptor(ReferenceFieldConstructor, flags, type);
            }
            else if ((flags & SyncFlags.Timestamp) != 0)
            {
                if (type != typeof(long))
                {
                    throw new NotSupportedException(string.Format("{0}.{1} must be used on type long", typeof(SyncFlags).Name, SyncFlags.Timestamp));
                }
                return new SyncFieldDescriptor(TimestampFieldConstructor, flags);
            }
            else
            {
                typeHandle = type.TypeHandle;
            }

            if ((flags & SyncFlags.HalfPrecision) != 0)
            {
                if (HalfConstructorLookups.Keys.Contains(typeHandle))
                {
                    return new SyncFieldDescriptor(HalfConstructorLookups[typeHandle], flags);
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
                if (ConstructorLookups.Keys.Contains(typeHandle))
                {
                    return new SyncFieldDescriptor(ConstructorLookups[typeHandle], flags);
                }
                else
                {
                    throw new NotSupportedException(string.Format("No SyncField registered for type {0}",type.Name));
                }
            }
        }
        
        internal static SyncFieldDescriptor GenerateFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            SyncFieldDescriptor descriptor = GenerateFieldDescriptorByType(fieldInfo.FieldType, syncFlags);
            descriptor.SetAccessors(
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo)
                );
            return descriptor;
        }
        
        internal static SyncFieldDescriptor GenerateFieldDescriptor(PropertyInfo propertyInfo, SyncFlags syncFlags)
        {
            SyncFieldDescriptor descriptor = GenerateFieldDescriptorByType(propertyInfo.PropertyType, syncFlags);
            descriptor.SetAccessors(
                DelegateGenerator.GenerateGetter(propertyInfo),
                DelegateGenerator.GenerateSetter(propertyInfo)
                );
            return descriptor;
        }
    }
}
