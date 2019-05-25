using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Synchronisers.Entities
{
    internal class FieldDescriptor
    {
        public SynchroniserFactory Factory { get; private set; }
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }

        public FieldDescriptor(SynchroniserFactory fieldFactory, Func<object, object> fieldGetter, Action<object, object> fieldSetter)
        {
            Factory = fieldFactory;
            Getter = fieldGetter;
            Setter = fieldSetter;
        }
    }
}
