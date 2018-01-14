using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace NetCode.SyncField
{
    internal class SyncFieldGenerator
    {
        Dictionary<RuntimeTypeHandle, Func<object>> FieldConstructorLookup = new Dictionary<RuntimeTypeHandle, Func<object>>();
        Dictionary<RuntimeTypeHandle, Func<object>> HalfPrecisionFieldConstructorLookup = new Dictionary<RuntimeTypeHandle, Func<object>>();

        internal SyncFieldGenerator()
        {
            RegisterFieldType(typeof(SynchronisableEnum), typeof(System.Enum));
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
        }

        internal SyncFieldDescriptor GenerateFieldDescriptor(FieldInfo fieldInfo, SyncFlags syncFlags)
        {
            Type type = fieldInfo.FieldType;

            if (type.BaseType == typeof(System.Enum))
            {
                type = typeof(System.Enum);
            }
            RuntimeTypeHandle typeHandle = type.TypeHandle;


            Func<object> constructor = null;

            if ((syncFlags & SyncFlags.HalfPrecisionFloats) != 0)
            {
                if (HalfPrecisionFieldConstructorLookup.Keys.Contains(typeHandle))
                {
                    constructor = HalfPrecisionFieldConstructorLookup[typeHandle];
                }

                if (constructor == null)
                    throw new NotSupportedException(string.Format("Type {0} not compatible with half precision.", type.Name));
            }
            else if (FieldConstructorLookup.Keys.Contains(typeHandle))
            {
                constructor = FieldConstructorLookup[typeHandle];

                if (constructor == null)
                {
                    throw new NotImplementedException(string.Format("Type {0} not synchronisable.", type.Name));
                }
            }

            return new SyncFieldDescriptor(
                constructor,
                DelegateGenerator.GenerateGetter(fieldInfo),
                DelegateGenerator.GenerateSetter(fieldInfo),
                syncFlags
                );
        }

        public void RegisterFieldType(Type syncFieldType, Type fieldType, SyncFlags flags = SyncFlags.None, bool overrideExistingFieldTypes = false)
        {
            if (syncFieldType.BaseType != typeof(SynchronisableField))
            {
                throw new NotSupportedException(string.Format(" {0} base type must be SynchronisableField.", syncFieldType.Name));
            }

            if (fieldType.BaseType == typeof(System.Enum))
            {
                fieldType = typeof(System.Enum);
            }
            RuntimeTypeHandle fieldTypeHandle = fieldType.TypeHandle;


            Func<object> constructor = DelegateGenerator.GenerateConstructor(syncFieldType);

            Dictionary<RuntimeTypeHandle, Func<object>> constructorLookup =
                ((flags & SyncFlags.HalfPrecisionFloats) != 0) ?
                HalfPrecisionFieldConstructorLookup : FieldConstructorLookup;

            if (!overrideExistingFieldTypes)
            {
                if (constructorLookup.ContainsKey(fieldTypeHandle))
                {
                    throw new NotSupportedException(string.Format("A SynchronisableField has already been registered against {0} with flags {1}", fieldType.Name, flags));
                }
            }

            constructorLookup[fieldTypeHandle] = constructor;
        }
    }
}
