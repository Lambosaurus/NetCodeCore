using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;


namespace NetCode.SyncField.Implementations
{   
    public class SyncFieldList<T> : SyncFieldContainer<T>
    {
        public SyncFieldList(SyncFieldFactory elementFactory, bool deltas) : base(elementFactory, deltas)
        {
        }

        public override object GetValue()
        {
            List<T> items = new List<T>(Elements.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                items.Add((T)Elements[i].GetValue());
            }
            return items;
        }

        public override bool TrackChanges(object newValue, SyncContext context)
        {
            bool changesFound = false;
            List<T> items = (List<T>)newValue;
            int count = items.Count;

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

    public class SyncFieldListFactory<T> : SyncFieldFactory
    {
        SyncFieldFactory ElementFactory;
        bool DeltaEncoding;
        public SyncFieldListFactory(SyncFieldFactory elementFactory, SyncFlags flags)
        {
            DeltaEncoding = (flags & SyncFlags.NoDeltas) == 0;
            ElementFactory = elementFactory;
        }

        public sealed override SynchronisableField Construct()
        {
            return new SyncFieldList<T>(ElementFactory, DeltaEncoding);
        }
    }
}
