using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;
using NetCode.Util;

using System.Reflection;

namespace NetCode.SyncField.Implementations
{   
    public class SynchronisableList<T> : SynchronisableField
    {
        private List<SynchronisableField> elements = new List<SynchronisableField>();

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

        private void SetElementLength(int count)
        {
            if (count > elements.Count)
            {
                byte childDepth = (byte)(ElementDepth + 1);
                for (int i = elements.Count; i < count; i++)
                {
                    elements.Add(Descriptor.GenerateField(childDepth));
                }
            }
            else
            {
                elements.RemoveRange(count, elements.Count - count);
            }
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
        
        public override void Read(byte[] data, ref int index)
        {
            byte count = Primitive.ReadByte(data, ref index);
            if (count != elements.Count) { SetElementLength(count); }
            foreach (SynchronisableField element in elements)
            {
                element.Read(data, ref index);
            }
        }
        
        public override void Skip(byte[] data, ref int index)
        {
            byte count = Primitive.ReadByte(data, ref index);
            throw new NotImplementedException();
        }
        
        public override void Write(byte[] data, ref int index)
        {
            Primitive.WriteByte(data, ref index, (byte)elements.Count);
            foreach ( SynchronisableField element in elements )
            {
                element.Write(data, ref index);
            }
        }

        public override int WriteToBufferSize()
        {
            int count = sizeof(byte);
            foreach (SynchronisableField element in elements)
            {
                count += element.WriteToBufferSize();
            }
            return count;
        }

        public override void PeriodicProcess(SyncContext context)
        {
            PollingRequired = false;
            foreach (SynchronisableField element in elements)
            {
                element.PeriodicProcess(context);
                if (element.PollingRequired) { PollingRequired = true; }
            }
        }

        public override void PostProcess(SyncContext context)
        {
            PollingRequired = false;
            foreach (SynchronisableField element in elements)
            {
                element.PostProcess(context);
                if (element.PollingRequired) { PollingRequired = true; }
            }
        }

        public override void PreProcess(SyncContext context)
        {
            foreach (SynchronisableField element in elements)
            {
                element.PreProcess(context);
            }
        }
    }
}
