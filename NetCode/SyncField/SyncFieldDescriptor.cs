using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.SyncField
{
    internal class SyncFieldDescriptor
    {
        SyncFlags flags;

        Func<object> constructor;
        public Type FieldType;
        public Func<object, object> Getter;
        public Action<object, object> Setter;
        
        public SyncFieldDescriptor(Func<object> fieldConstructor, Func<object, object> fieldGetter, Action<object, object> fieldSetter, SyncFlags syncFlags, Type fieldType)
        {
            constructor = fieldConstructor;
            Getter = fieldGetter;
            Setter = fieldSetter;
            flags = syncFlags;
            FieldType = fieldType;
        }

        public SynchronisableField GenerateField()
        {
            SynchronisableField field = (SynchronisableField)(constructor.Invoke());
            field.Flags = flags;
            field.FieldType = FieldType;
            return field;
        }
    }
}
