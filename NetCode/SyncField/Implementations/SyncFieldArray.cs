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
        public override object GetValue()
        {
            T[] items = new T[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                items[i] = (T)elements[i].GetValue();
            }
            return items;
        }

        public override bool ValueEqual(object newValue)
        {
            T[] items = (T[])newValue;
            if (items.Length != elements.Count)
            {
                return false;
            }
            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].ValueEqual(items[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
        public override void SetValue(object newValue)
        {
            T[] items = (T[])newValue;
            if (items.Length != elements.Count) { SetElementLength(items.Length); }
            for (int i = 0; i < items.Length; i++)
            {
                elements[i].SetValue(items[i]);
            }
        }
    }
}
