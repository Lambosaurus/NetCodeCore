using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;
using NetCode.Util;

using System.Reflection;

namespace NetCode.SyncField.Implementations
{   
    public class SynchronisableArray<T> : SynchronisableContainer<T>
    {
        public SynchronisableArray(SyncFieldFactory elementFactory) : base(elementFactory)
        {
        }

        public override object GetValue()
        {
            T[] items = new T[Elements.Count];
            for (int i = 0; i < Elements.Count; i++)
            {
                items[i] = (T)Elements[i].GetValue();
            }
            return items;
        }

        public override bool TrackChanges(object newValue, SyncContext context)
        {
            bool changesFound = false;
            T[] items = (T[])newValue;
            int count = items.Length;

            if (Elements.Count != count)
            {
                changesFound = true;
                SetElementLength(count);
            }
            
            for (int i = 0; i < count; i++)
            {
                if (Elements[i].TrackChanges(items[i], context))
                {
                    changesFound = true;
                }
            }

            if (changesFound)
            {
                Revision = context.Revision;
            }
            return changesFound;
        }
    }
}
