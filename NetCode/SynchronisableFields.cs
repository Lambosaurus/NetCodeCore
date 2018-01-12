using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using System.Reflection.Emit;

namespace NetCode
{
    public abstract class SynchronisableField
    {
        public bool Changed { get; private set; } = true; // Defaults to true so value is changed when created
        public uint LastPacketUUID { get; private set; } = 0;

        public void Update(object new_value)
        {
            if (ValueEqual(new_value))
            {
                Changed = true;
                SetValue(new_value);
            }
        }

        public void WriteToPacket(byte[] data, ref int index, uint packet_uuid)
        {
            Write(data, ref index);
            Changed = false;
            LastPacketUUID = packet_uuid;
        }


        /// <summary>
        /// Gets the internal value of the field
        /// </summary>
        /// <param name="new_value"></param>
        protected abstract void SetValue(object new_value);

        /// <summary>
        /// Sets the internal value of the field
        /// </summary>
        protected abstract object GetValue();

        /// <summary>a
        /// Returns true if the new value does not match the stored value.
        /// </summary>
        /// <param name="new_value"></param>
        protected abstract bool ValueEqual(object new_value);

        /// <summary>
        /// Returns the number of bytes required by Write()
        /// This returns true whether or not the value has changed.
        /// </summary>
        public abstract int WriteSize();

        /// <summary>
        /// Writes the Synchronisable value into the packet.
        /// </summary>
        /// <param name="data"> The packet to write to </param>
        /// <param name="index"> The index to begin writing at. The index will be incremented by the number of bytes written </param>
        protected abstract void Write(byte[] data, ref int index);

        /// <summary>
        /// Reads the Synchronisable value from the packet.
        /// </summary>
        /// <param name="data"> The packet to read from </param>
        /// <param name="index"> The index to begin reading at. The index will be incremented by the number of bytes written </param>
        protected abstract void Read(byte[] data, ref int index);
    }


    internal class SyncFieldDescriptor
    {
        SyncFlags flags;

        Func<object> constructor;
        public Func<object, object> Getter;
        public Action<object, object> Setter;

        public SyncFieldDescriptor( Func<object> fieldConstructor, Func<object,object> fieldGetter, Action<object, object> fieldSetter, SyncFlags syncFlags )
        {
            constructor = fieldConstructor;
            Getter = fieldGetter;
            Setter = fieldSetter;
            flags = syncFlags;
        }
        
        public SynchronisableField GenerateField()
        {
            return (SynchronisableField)(constructor.Invoke());
        }
    }


    internal class SyncFieldgenerator
    {
        Dictionary<RuntimeTypeHandle, Func<object>> FieldConstructorLookup = new Dictionary<RuntimeTypeHandle, Func<object>>();
        Dictionary<RuntimeTypeHandle, Func<object>> HalfPrecisionFieldConstructorLookup = new Dictionary<RuntimeTypeHandle, Func<object>>();
       
        internal SyncFieldgenerator()
        {
            RegisterSynchronisableFieldType(typeof(SynchronisableEnum), typeof(System.Enum));
            RegisterSynchronisableFieldType(typeof(SynchronisableByte), typeof(byte));
            RegisterSynchronisableFieldType(typeof(SynchronisableShort), typeof(short));
            RegisterSynchronisableFieldType(typeof(SynchronisableUShort), typeof(ushort));
            RegisterSynchronisableFieldType(typeof(SynchronisableInt), typeof(int));
            RegisterSynchronisableFieldType(typeof(SynchronisableUInt), typeof(uint));
            RegisterSynchronisableFieldType(typeof(SynchronisableLong), typeof(long));
            RegisterSynchronisableFieldType(typeof(SynchronisableULong), typeof(ulong));
            RegisterSynchronisableFieldType(typeof(SynchronisableFloat), typeof(float));
            RegisterSynchronisableFieldType(typeof(SynchronisableString), typeof(string));
            RegisterSynchronisableFieldType(typeof(SynchronisableHalf), typeof(float), SyncFlags.HalfPrecisionFloats);
        }
        
        internal SyncFieldDescriptor GenerateFieldDescriptor( FieldInfo fieldInfo, SyncFlags syncFlags)
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

        public void RegisterSynchronisableFieldType( Type syncFieldType, Type fieldType, SyncFlags flags = SyncFlags.None, bool overrideExistingFieldTypes = false )
        {
            if ( !(syncFieldType.BaseType.Equals(typeof(SynchronisableField)) ) )
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

    public class SynchronisableEnum : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)(int)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (byte)(int)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadByte(data, ref index); }
    }

    public class SynchronisableByte : SynchronisableField
    {
        internal byte value;
        protected override void SetValue(object new_value) { value = (byte)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (byte)new_value == value; }
        public override int WriteSize() { return sizeof(byte); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadByte(data, ref index); }
    }

    public class SynchronisableShort : SynchronisableField
    {
        internal short value;
        protected override void SetValue(object new_value) { value = (short)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (short)new_value == value; }
        public override int WriteSize() { return sizeof(short); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadShort(data, ref index); }
    }

    public class SynchronisableUShort : SynchronisableField
    {
        internal ushort value;
        protected override void SetValue(object new_value) { value = (ushort)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ushort)new_value == value; }
        public override int WriteSize() { return sizeof(ushort); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadUShort(data, ref index); }
    }

    public class SynchronisableInt : SynchronisableField
    {
        internal int value;
        protected override void SetValue(object new_value) { value = (int)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (int)new_value == value; }
        public override int WriteSize() { return sizeof(int); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadInt(data, ref index); }
    }

    public class SynchronisableUInt : SynchronisableField
    {
        internal uint value;
        protected override void SetValue(object new_value) { value = (uint)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (uint)new_value == value; }
        public override int WriteSize() { return sizeof(uint); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadUInt(data, ref index); }
    }

    public class SynchronisableLong : SynchronisableField
    {
        internal long value;
        protected override void SetValue(object new_value) { value = (long)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (long)new_value == value; }
        public override int WriteSize() { return sizeof(long); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadLong(data, ref index); }
    }

    public class SynchronisableULong : SynchronisableField
    {
        internal ulong value;
        protected override void SetValue(object new_value) { value = (ulong)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (ulong)new_value == value; }
        public override int WriteSize() { return sizeof(ulong); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadULong(data, ref index); }
    }

    public class SynchronisableFloat : SynchronisableField
    {
        internal float value;
        protected override void SetValue(object new_value) { value = (float)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (float)new_value == value; }
        public override int WriteSize() { return sizeof(float); }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadFloat(data, ref index); }
    }

    

    public class SynchronisableString : SynchronisableField
    {
        internal string value;
        protected override void SetValue(object new_value) { value = (string)new_value; }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (string)new_value == value; }
        public override int WriteSize() { return value.Length + 1; }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadString(data, ref index); }
    }

    public class SynchronisableHalf : SynchronisableField
    {
        internal Half value;
        protected override void SetValue(object new_value) { value = (Half)((float)new_value); }
        protected override object GetValue() { return value; }
        protected override bool ValueEqual(object new_value) { return (Half)((float)new_value) == value; }
        public override int WriteSize() { return 2; }
        protected override void Write(byte[] data, ref int index) { PrimitiveSerialiser.Write(data, ref index, value); }
        protected override void Read(byte[] data, ref int index) { value = PrimitiveSerialiser.ReadHalf(data, ref index); }
    }
}
