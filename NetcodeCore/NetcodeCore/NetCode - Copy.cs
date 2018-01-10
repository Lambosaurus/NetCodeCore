using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

namespace NetcodeCore
{
    
    public class SynchronisableAttribute : System.Attribute
    {
        public enum Flags { None = 0, HalfPrecisionFloats = 1 };

        Flags flags;
        public SynchronisableAttribute(Flags _flags = Flags.None)
        {
            flags = _flags;
        }
    }
    


    
    public class NetCode
    {
        const int PACKET_HEADER_SIZE = sizeof(uint);
        
        struct ObjectStatePair
        {
            public object obj;
            public SynchronisableObjectState state;
        }

        List<ObjectStatePair> ObjectStates = new List<ObjectStatePair>();
        Dictionary<string, SynchronisableObjectDescriptor> templates = new Dictionary<string, SynchronisableObjectDescriptor>();

        uint last_object_id = 0;
        private uint GetNewObjectId()
        {
            return last_object_id++;
        }

        uint last_packet_id = 0;
        private uint GetNewPacketId()
        {
            return last_packet_id++;
        }

        public void RegisterType(Type sync_type)
        {
            string name = sync_type.Name;
            templates[name] = new SynchronisableObjectDescriptor(sync_type);
        }

        public void RegisterInstance(object instance)
        {
            string name = instance.GetType().Name;
            ObjectStatePair pair = new ObjectStatePair();

            pair.obj = instance;
            pair.state = new SynchronisableObjectState(templates[name], GetNewObjectId());
            ObjectStates.Add(pair);
        }

        private void WriteHeader(byte[] data, ref int index)
        {
            PrimitiveSerialiser.Write(data, ref index, GetNewPacketId());
        }
        
        public void Pull()
        {
            foreach ( ObjectStatePair pair in ObjectStates)
            {
                pair.state.Pull(pair.obj);
            }
        }

        public void Transfter()
        {
            int packetsize = PACKET_HEADER_SIZE;
            foreach (ObjectStatePair pair in ObjectStates)
            {
                packetsize += pair.state.WriteSize();
            }

            int index = 0;
            byte[] data = new byte[packetsize];

            WriteHeader(data, ref index);

            foreach (ObjectStatePair pair in ObjectStates)
            {
                pair.state.Write(data, ref index);
            }
        }

        public void Push()
        {
            foreach (ObjectStatePair pair in ObjectStates)
            {
                pair.state.Push(pair.obj);
            }
        }


        class SynchronisableObjectState
        {
            const int UUID_HEADER_LENGTH = sizeof(uint);
            const int FIELD_HEADER_LENGTH = sizeof(byte);

            SynchronisableObjectDescriptor template;
            List<SynchronisableField> fields;
            
            uint uuid;
            bool changed = false;
            
            public SynchronisableObjectState( SynchronisableObjectDescriptor arg_template, uint id)
            {
                template = arg_template;
                uuid = id;

                fields = new List<SynchronisableField>();

                foreach (FieldInfo info in template.fieldinfo)
                {
                    Type vartype = info.FieldType;
                    fields.Add(new SynchronisableField(vartype));
                }
            }

            public int WriteSize()
            {
                if (!changed) { return 0; }

                int size = UUID_HEADER_LENGTH;
                foreach (SynchronisableField field in fields)
                {
                    if (field.changed)
                    {
                        size += FIELD_HEADER_LENGTH + field.VarSize();
                    }
                }
                return size;
            }

            // Write also clears any changed flags
            public void Write(byte[] data, ref int index)
            {
                if (!changed) { return; }

                PrimitiveSerialiser.Write(data, ref index, uuid);
                
                for (byte i = 0; i < fields.Count; i++)
                {
                    SynchronisableField field = fields[i];
                    if (field.changed)
                    {
                        // This MUST be written as a byte.
                        PrimitiveSerialiser.Write(data, ref index, (byte)i);
                        field.Write(data, ref index);
                        field.changed = false;
                    }
                }

                changed = false;
            }
            
            public void Push( object obj )
            {
                for (int i = 0; i < template.fieldinfo.Count; i++)
                {
                    template.fieldinfo[i].SetValue(obj, fields[i++].Get());
                }
            }
            
            public void Pull(object obj)
            {
                for (int i = 0; i < template.fieldinfo.Count; i++)
                {
                    object value = template.fieldinfo[i].GetValue(obj);
                    fields[i].Set(value);
                    if (fields[i].changed) { changed = true; }
                }
            }
        }


        public class SynchronisableField
        {
            public enum FieldType {None, Byte, Short, UShort, Int, UInt, Long, ULong, Float, Vector, String, HalfFloat, HalfVector};

            public FieldType value_type;
            public object value;
            public bool changed = false;

            public SynchronisableField(FieldType type)
            {
                value_type = type;
                DefaultValue();
            }

            public SynchronisableField(Type type, SynchronisableAttribute.Flags flags = SynchronisableAttribute.Flags.None)
            {
                value_type = SyncTypeFromType(type, flags);
                if (value_type == FieldType.None) { throw new NotImplementedException( string.Format("Type {0} cannot be synchronised.", type.Name) ); }
                DefaultValue();
            }

            public void DefaultValue()
            {
                switch (value_type)
                {
                    case (FieldType.Byte): { value = (byte)0; break; }
                }
                if (value_type == FieldType.Byte) { value = (byte)0; }
                if (value_type == FieldType.Short) { value = (short)0; }
                if (value_type == FieldType.UShort) { value = (ushort)0; }
                if (value_type == FieldType.Int) { value = (int)0; }
                if (value_type == FieldType.UInt) { value = (uint)0; }
                if (value_type == FieldType.Long) { value = (long)0; }
                if (value_type == FieldType.ULong) { value = (ulong)0; }
                if (value_type == FieldType.Float) { value = 0f; }
                if (value_type == FieldType.Vector) { value = Vector2.Zero; }
                if (value_type == FieldType.String) { value = ""; }
                if (value_type == FieldType.HalfFloat) { value = (Half)0f; }
                if (value_type == FieldType.HalfVector) { value = new HalfVector2(); }
            }

            public static FieldType SyncTypeFromType(Type type, SynchronisableAttribute.Flags flags = SynchronisableAttribute.Flags.None)
            {
                if (type.BaseType == typeof(System.Enum)) { return FieldType.Byte; }
                if (type == typeof(byte)) { return FieldType.Byte; }
                if (type == typeof(short)) { return FieldType.Short; }
                if (type == typeof(ushort)) { return FieldType.UShort; }
                if (type == typeof(int)) { return FieldType.Int; }
                if (type == typeof(uint)) { return FieldType.UInt; }
                if (type == typeof(long)) { return FieldType.Long; }
                if (type == typeof(ulong)) { return FieldType.ULong; }
                if (type == typeof(float))
                {
                    if ((flags & SynchronisableAttribute.Flags.HalfPrecisionFloats) != 0) { return FieldType.HalfFloat; }
                    return FieldType.Float;
                }
                if (type == typeof(Vector2))
                {
                    if ((flags & SynchronisableAttribute.Flags.HalfPrecisionFloats) != 0) { return FieldType.HalfVector; }
                    return FieldType.Vector;
                }
                if (type == typeof(string)) { return FieldType.String; }
                return FieldType.None;
            }
            
            public void Set( object new_value )
            {
                if (value_type == FieldType.Byte) { if ((byte)new_value != (byte)value) { changed = true; } }
                if (value_type == FieldType.Short) { if ((short)new_value != (short)value) { changed = true; } }
                if (value_type == FieldType.UShort) { if ((ushort)new_value != (ushort)value) { changed = true; } }
                if (value_type == FieldType.Int) { if ((int)new_value != (int)value) { changed = true; } }
                if (value_type == FieldType.UInt) { if ((uint)new_value != (uint)value) { changed = true; } }
                if (value_type == FieldType.Long) { if ((long)new_value != (long)value) { changed = true; } }
                if (value_type == FieldType.ULong) { if ((ulong)new_value != (ulong)value) { changed = true; } }
                if (value_type == FieldType.Float) { if ((float)new_value != (float)value) { changed = true; } }
                if (value_type == FieldType.Vector) { if ((Vector2)new_value != (Vector2)value) { changed = true; } }
                if (value_type == FieldType.String) { if ((string)new_value != (string)value) { changed = true; } }
                if (value_type == FieldType.HalfFloat) { if ((Half)((float)new_value) != (Half)value) { changed = true; } }
                if (value_type == FieldType.HalfVector) { if (!(new HalfVector2((Vector2)new_value).Equals((HalfVector2)value))) { changed = true; } }
                value = new_value;
            }

            public object Get()
            {
                return value;
            }

            public int VarSize()
            {
                if (value_type == FieldType.Byte) { return sizeof(byte); }
                if (value_type == FieldType.Short) { return sizeof(short); }
                if (value_type == FieldType.UShort) { return sizeof(ushort); }
                if (value_type == FieldType.Int) { return sizeof(int); }
                if (value_type == FieldType.UInt) { return sizeof(uint); }
                if (value_type == FieldType.Long) { return sizeof(long); }
                if (value_type == FieldType.ULong) { return sizeof(ulong); }
                if (value_type == FieldType.Float) { return sizeof(float); }
                if (value_type == FieldType.Vector) { return sizeof(float)*2; }
                if (value_type == FieldType.String) { return ((string)value).Length; }
                if (value_type == FieldType.HalfFloat) { return 2; }
                if (value_type == FieldType.HalfVector) { return 4; }
                return 0;
            }
            
            public void Write(byte[] data, ref int index)
            {
                if (value_type == FieldType.Byte) { PrimitiveSerialiser.Write(data, ref index, (byte)value); }
                else if (value_type == FieldType.Short) { PrimitiveSerialiser.Write(data, ref index, (short)value); }
                else if (value_type == FieldType.UShort) { PrimitiveSerialiser.Write(data, ref index, (ushort)value); }
                else if (value_type == FieldType.Int) { PrimitiveSerialiser.Write(data, ref index, (int)value); }
                else if (value_type == FieldType.UInt) { PrimitiveSerialiser.Write(data, ref index, (uint)value); }
                else if (value_type == FieldType.Long) { PrimitiveSerialiser.Write(data, ref index, (long)value); }
                else if (value_type == FieldType.ULong) { PrimitiveSerialiser.Write(data, ref index, (ulong)value); }
                else if (value_type == FieldType.Float) { PrimitiveSerialiser.Write(data, ref index, (float)value); }
                else if (value_type == FieldType.Vector) { PrimitiveSerialiser.Write(data, ref index, (Vector2)value); }
                else if (value_type == FieldType.String) { PrimitiveSerialiser.Write(data, ref index, (string)value); }
                else if (value_type == FieldType.HalfFloat) { PrimitiveSerialiser.Write(data, ref index, (Half)value); }
                else if (value_type == FieldType.HalfVector) { PrimitiveSerialiser.Write(data, ref index, (HalfVector2)value); }
            }
        }

        private class SynchronisableObjectDescriptor
        {
            public List<FieldInfo> fieldinfo = new List<FieldInfo>();

            public SynchronisableObjectDescriptor(Type sync_type)
            {
                foreach (FieldInfo info in sync_type.GetFields())
                {
                    foreach (object attribute in info.GetCustomAttributes(true))
                    {
                        if (attribute is SynchronisableAttribute)
                        {
                            if (  SynchronisableField.SyncTypeFromType(info.FieldType) != SynchronisableField.FieldType.None)
                            {
                                fieldinfo.Add(info);
                            }
                        }
                    }
                }
            }
        }

        internal struct HalfVector2
        {
            public Half X;
            public Half Y;

            public HalfVector2(Vector2 vector)
            {
                X = (Half)vector.X;
                Y = (Half)vector.Y;
            }

            public bool Equals(HalfVector2 other)
            {
                return this.X == other.X && this.Y == other.Y;
            }
        }

        private class PrimitiveSerialiser
        {
            public static void Write(byte[] data, ref int index, byte value)
            {
                data[index++] = value;
            }
            public static void Write(byte[] data, ref int index, short value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, ushort value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, int value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, uint value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, long value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, ulong value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, float value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, Vector2 value)
            {
                Write(data, ref index, value.X);
                Write(data, ref index, value.Y);
            }
            public static void Write(byte[] data, ref int index, string value)
            {
                data[index++] = (byte)value.Length;
                foreach (char ch in value) { data[index++] = (byte)ch; }
            }
            public static void Write(byte[] data, ref int index, Half value)
            {
                byte[] bytes = Half.GetBytes(value);
                foreach (byte b in bytes) { data[index++] = b; }
            }
            public static void Write(byte[] data, ref int index, HalfVector2 value)
            {
                Write(data, ref index, value.X);
                Write(data, ref index, value.Y);
            }
        }
    }
}
