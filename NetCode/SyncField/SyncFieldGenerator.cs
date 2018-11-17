﻿using System;
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
        
        private SyncFieldDescriptor GenerateFieldDescriptorByType(Type type, SyncFlags flags)
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
                elementcontent.InsertParentConstructor(DelegateGenerator.GenerateConstructor(syncListType));
                return elementcontent;
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                Type syncListType = typeof(SynchronisableArray<>).MakeGenericType(new Type[] { elementType });
                SyncFieldDescriptor elementcontent = GenerateFieldDescriptorByType(elementType, flags);
                elementcontent.InsertParentConstructor(DelegateGenerator.GenerateConstructor(syncListType));
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

        internal SyncFieldDescriptor GenerateFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            SyncFieldDescriptor descriptor = GenerateFieldDescriptorByType(fieldInfo.FieldType, syncFlags);
            descriptor.SetAccessors(
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo)
                );
            return descriptor;
        }


        internal SyncFieldDescriptor GenerateFieldDescriptor(PropertyInfo propertyInfo, SyncFlags syncFlags)
        {
            SyncFieldDescriptor descriptor = GenerateFieldDescriptorByType(propertyInfo.PropertyType, syncFlags);
            descriptor.SetAccessors(
                DelegateGenerator.GenerateGetter(propertyInfo),
                DelegateGenerator.GenerateSetter(propertyInfo)
                );
            return descriptor;
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
