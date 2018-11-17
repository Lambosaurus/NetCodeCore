using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;
using NetCode.Util;

using System.Reflection;

namespace NetCode.SyncField.Implementations
{   
    public class SynchronisableList<T> : SynchronisableContainer<T>
    {
        public override object GetValue()
        {
            List<T> items = new List<T>(elements.Count);
            foreach (SynchronisableField element in elements)
            {
                items.Add((T)element.GetValue());
            }
            return items;
        }

        public override bool ValueEqual(object newValue)
        {
            List<T> items = (List<T>)newValue;
            if (items.Count != elements.Count)
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
            List<T> items = (List<T>)newValue;
            if (items.Count != elements.Count) { SetElementLength(items.Count); }
            for (int i = 0; i < items.Count; i++)
            {
                elements[i].SetValue(items[i]);
            }
        }
    }
}
