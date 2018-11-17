using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using NetCode.Util;
using NetCode.SyncField.Implementations;

namespace NetCode.SyncField
{
    internal class SyncFieldGenerator
    {
        private Dictionary<RuntimeTypeHandle, Func<object>> HalfConstructorLookups = new Dictionary<RuntimeTypeHandle, Func<object>>();
        private Dictionary<RuntimeTypeHandle, Func<object>> ConstructorLookups = new Dictionary<RuntimeTypeHandle, Func<object>>();
        private static Func<object> TimestampFieldConstructor;
        private static Func<object> ReferenceFieldConstructor;

        static SyncFieldGenerator()
        {
            TimestampFieldConstructor = DelegateGenerator.GenerateConstructor(typeof(SynchronisableTimestamp));
            ReferenceFieldConstructor = DelegateGenerator.GenerateConstructor(typeof(SynchronisableReference));
        }
        
        internal SyncFieldGenerator()
        {
            RegisterDefaultFieldTypes();
        }

        private void RegisterDefaultFieldTypes()
        {
            RegisterFieldType(typeof(SynchronisableEnum), typeof(System.Enum));
            RegisterFieldType(typeof(SynchronisableBool), typeof(bool));
            RegisterFieldType(typeof(SynchronisableByte), typeof(byte));
            RegisterFieldType(typeof(SynchronisableShort), typeof(short));
            RegisterFieldType(typeof(SynchronisableUShort), typeof(ushort));
            RegisterFieldType(typeof(SynchronisableInt), typeof(int));
            RegisterFieldType(typeof(SynchronisableUInt), typeof(uint));
            RegisterFieldType(typeof(SynchronisableLong), typeof(long));
            RegisterFieldType(typeof(SynchronisableULong), typeof(ulong));
            RegisterFieldType(typeof(SynchronisableFloat), typeof(float));
            RegisterFieldType(typeof(SynchronisableDouble), typeof(double));
            RegisterFieldType(typeof(SynchronisableString), typeof(string));
            RegisterFieldType(typeof(SynchronisableHalf), typeof(float), SyncFlags.HalfPrecision);
            RegisterFieldType(typeof(SynchronisableFloat), typeof(double), SyncFlags.HalfPrecision);
        }

        private struct FieldConstructorLookupContent {
            
            public FieldConstructorLookupContent( Func<object> ctor, Type resolvingType = null )
            {
                Constructors = new Func<object>[] { ctor };
                ResolvingType = resolvingType;
            }

            public void InsertConstructor(Func<object> ctor)
            {
                Func<object>[] ctors = new Func<object>[Constructors.Length + 1];
                ctors[0] = ctor;
                Constructors.CopyTo(ctors, 1);
                Constructors = ctors;
            }

            public Func<object>[] Constructors;
            public Type ResolvingType;
        }

        private FieldConstructorLookupContent LookupSyncFieldConstructor(Type type, SyncFlags syncFlags)
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
                FieldConstructorLookupContent elementcontent = LookupSyncFieldConstructor(elementType, syncFlags);
                elementcontent.InsertConstructor(DelegateGenerator.GenerateConstructor(syncListType));
                return elementcontent;
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                Type syncListType = typeof(SynchronisableArray<>).MakeGenericType(new Type[] { elementType });
                FieldConstructorLookupContent elementcontent = LookupSyncFieldConstructor(elementType, syncFlags);
                elementcontent.InsertConstructor(DelegateGenerator.GenerateConstructor(syncListType));
                return elementcontent;
            }
            else if ((syncFlags & SyncFlags.Reference) != 0)
            {
                if (type.IsValueType)
                {
                    throw new NotSupportedException(string.Format("{0}.{1} can not be used on ValueType", typeof(SyncFlags).Name, SyncFlags.Reference));
                }
                return new FieldConstructorLookupContent(ReferenceFieldConstructor, type);
            }
            else if ((syncFlags & SyncFlags.Timestamp) != 0)
            {
                if (type != typeof(long))
                {
                    throw new NotSupportedException(string.Format("{0}.{1} must be used on type long", typeof(SyncFlags).Name, SyncFlags.Timestamp));
                }
                return new FieldConstructorLookupContent(TimestampFieldConstructor);
            }
            else
            {
                typeHandle = type.TypeHandle;
            }

            if ((syncFlags & SyncFlags.HalfPrecision) != 0)
            {
                if (HalfConstructorLookups.Keys.Contains(typeHandle))
                {
                    return new FieldConstructorLookupContent(HalfConstructorLookups[typeHandle]);
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
                    return new FieldConstructorLookupContent(ConstructorLookups[typeHandle]);
                }
                else
                {
                    throw new NotSupportedException(string.Format("No SyncField registered for type {0}",type.Name));
                }
            }
        }

        internal SyncFieldDescriptor GenerateFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            FieldConstructorLookupContent fieldContent = LookupSyncFieldConstructor(fieldInfo.FieldType, syncFlags);
            return new SyncFieldDescriptor(
                fieldContent.Constructors,
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo),
                syncFlags,
                fieldContent.ResolvingType
                );
        }


        internal SyncFieldDescriptor GenerateFieldDescriptor(PropertyInfo propertyInfo, SyncFlags syncFlags)
        {
            FieldConstructorLookupContent fieldContent = LookupSyncFieldConstructor(propertyInfo.PropertyType, syncFlags);
            return new SyncFieldDescriptor(
                fieldContent.Constructors,
                DelegateGenerator.GenerateGetter(propertyInfo),
                DelegateGenerator.GenerateSetter(propertyInfo),
                syncFlags,
                fieldContent.ResolvingType
                );
        }

        public void RegisterFieldType(Type syncFieldType, Type fieldType, SyncFlags syncFlags = SyncFlags.None, bool overrideExistingFieldTypes = false)
        {
            
            if (!syncFieldType.IsSubclassOf(typeof(SynchronisableField)))
            {
                throw new NotSupportedException(string.Format(
                    "{0} must inherit from SynchronisableField.",
                    syncFieldType.Name
                    ));
            }
            
            if (fieldType.IsEnum) { fieldType = typeof(System.Enum); }
            RuntimeTypeHandle fieldTypeHandle = fieldType.TypeHandle;
            
            Func<object> constructor = DelegateGenerator.GenerateConstructor(syncFieldType);
            
            Dictionary<RuntimeTypeHandle, Func<object>> lookup = ((syncFlags & SyncFlags.HalfPrecision) != 0)
                                                               ? HalfConstructorLookups : ConstructorLookups;

            if (!overrideExistingFieldTypes)
            {
                if (lookup.ContainsKey(fieldTypeHandle))
                {
                    throw new NotSupportedException(string.Format(
                        "A SynchronisableField has already been registered against {0} with flags {1}",
                        fieldType.Name, syncFlags
                        ));
                }
            }

            lookup[fieldTypeHandle] = constructor;
        }
    }
}
