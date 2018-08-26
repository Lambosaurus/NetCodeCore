using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using NetCode.Util;
using NetCode.SyncField.Implementations;

namespace NetCode.SyncField
{
    internal class SyncFieldGenerator
    {
        Dictionary<SyncFlags, Dictionary<RuntimeTypeHandle, Func<object>>> ConstructorLookups = new Dictionary<SyncFlags, Dictionary<RuntimeTypeHandle, Func<object>>>();

        // These are flags that represent a unique constructor definition
        private SyncFlags[] ConstructorFlags = new SyncFlags[]
        {
            SyncFlags.Reference,
            SyncFlags.Timestamp,
            SyncFlags.HalfPrecisionFloats,
            SyncFlags.None, // None functions as a fallback.
        };

        internal SyncFieldGenerator()
        {
            RegisterDefaultFieldTypes();
        }

        private void RegisterDefaultFieldTypes()
        {
            foreach (SyncFlags flag in ConstructorFlags)
            {
                ConstructorLookups[flag] = new Dictionary<RuntimeTypeHandle, Func<object>>();
            }
            
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
            RegisterFieldType(typeof(SynchronisableHalf), typeof(float), SyncFlags.HalfPrecisionFloats);
            RegisterFieldType(typeof(SynchronisableFloat), typeof(double), SyncFlags.HalfPrecisionFloats);
            RegisterFieldType(typeof(SynchronisableTimestamp), typeof(long), SyncFlags.Timestamp);
            RegisterFieldType(typeof(SynchronisableReference), typeof(object), SyncFlags.Reference);
        }

        internal Func<object> LookupConstructorForSyncField(Type type, SyncFlags syncFlags)
        {
            if (type.BaseType == typeof(System.Enum)) { type = typeof(System.Enum); }
            else if ((syncFlags & SyncFlags.Reference) != 0 && !type.IsValueType)
            {
                type = typeof(object);
            }

            RuntimeTypeHandle typeHandle = type.TypeHandle;

            foreach (SyncFlags flag in ConstructorFlags)
            {
                if ((syncFlags & flag) == flag)
                {
                    if ( ConstructorLookups[flag].Keys.Contains(typeHandle) )
                    {
                        return ConstructorLookups[flag][typeHandle];
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format(
                            "Type {0} not compatible with {1}.{2}.",
                            type.Name, typeof(SyncFlags).Name, flag
                            ));
                    }
                }
            }

            return null; // Control should never reach this.
        }

        internal SyncFieldDescriptor GenerateFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            return new SyncFieldDescriptor( 
                LookupConstructorForSyncField(fieldInfo.FieldType, syncFlags),
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo),
                syncFlags,
                fieldInfo.FieldType
                );
        }


        internal SyncFieldDescriptor GenerateFieldDescriptor(PropertyInfo propertyInfo, SyncFlags syncFlags)
        {
            return new SyncFieldDescriptor(
                LookupConstructorForSyncField(propertyInfo.PropertyType, syncFlags),
                DelegateGenerator.GenerateGetter(propertyInfo),
                DelegateGenerator.GenerateSetter(propertyInfo),
                syncFlags,
                propertyInfo.PropertyType
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
            
            Dictionary<RuntimeTypeHandle, Func<object>> constructorLookup = null;
            foreach (SyncFlags flag in ConstructorFlags)
            {
                // The equality check to flag will always pass for SyncFlags.None
                if ((syncFlags & flag) == flag)
                {
                    constructorLookup = ConstructorLookups[flag];
                    break;
                }
            }
            
            if (!overrideExistingFieldTypes)
            {
                if (constructorLookup.ContainsKey(fieldTypeHandle))
                {
                    throw new NotSupportedException(string.Format(
                        "A SynchronisableField has already been registered against {0} with flags {1}",
                        fieldType.Name, syncFlags
                        ));
                }
            }

            constructorLookup[fieldTypeHandle] = constructor;
        }
    }
}
