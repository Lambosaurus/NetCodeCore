using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.SyncField
{
    internal class SyncFieldDescriptor
    {
        public SyncFlags Flags { get; private set; }
        public Type ReferenceType { get; private set; }
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }

        private Func<SynchronisableField>[] Constructors;

        public SyncFieldDescriptor(Func<SynchronisableField> fieldConstructor, SyncFlags syncFlags, Type referenceType = null)
        {
            Constructors = new Func<SynchronisableField>[] { fieldConstructor };
            Flags = syncFlags;
            ReferenceType = referenceType;
        }
        
        public void InsertParentConstructor(Func<SynchronisableField> fieldConstructor)
        {
            Func<SynchronisableField>[] newConstructors = new Func<SynchronisableField>[Constructors.Length + 1];
            newConstructors[0] = fieldConstructor;
            Constructors.CopyTo(newConstructors, 1);
            Constructors = newConstructors;
        }

        public void SetAccessors(Func<object, object> fieldGetter, Action<object, object> fieldSetter)
        {
            Getter = fieldGetter;
            Setter = fieldSetter;
        }

        public SynchronisableField GenerateField(byte elementDepth = 0)
        {
            SynchronisableField field = (Constructors[elementDepth].Invoke());
            field.Initialise(this, elementDepth);
            return field;
        }
    }
}
