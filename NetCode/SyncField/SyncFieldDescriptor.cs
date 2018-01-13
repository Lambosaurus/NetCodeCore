﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField
{
    internal class SyncFieldDescriptor
    {
        SyncFlags flags;

        Func<object> constructor;
        public Func<object, object> Getter;
        public Action<object, object> Setter;

        public SyncFieldDescriptor(Func<object> fieldConstructor, Func<object, object> fieldGetter, Action<object, object> fieldSetter, SyncFlags syncFlags)
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
}