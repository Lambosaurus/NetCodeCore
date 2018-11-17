using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;
using NetCode.Util;

using System.Reflection;

namespace NetCode.SyncField.Implementations
{   
    public abstract class SynchronisableContainer<T> : SynchronisableField
    {
        protected List<SynchronisableField> elements = new List<SynchronisableField>();
        protected SynchronisableField skipElement;
        
        protected void SetElementLength(int count)
        {
            if (count > elements.Count)
            {
                // This may be bad for streamed updates.....
                if (elements.Count == 0 && skipElement != null)
                {
                    // Reuse the skip element if possible to prevent waste.
                    elements.Add(skipElement);
                }
                
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
            if (count > 0)
            {
                if (skipElement == null)
                {
                    // We need an instance of the sub element to skip correctly.
                    // We generate this on demand only because only few SyncLists will need to skip items
                    if (elements.Count == 0)
                    {
                        byte childDepth = (byte)(ElementDepth + 1);
                        skipElement = Descriptor.GenerateField(childDepth);
                    }
                    else
                    {
                        skipElement = elements[0];
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    skipElement.Skip(data, ref index);
                }
            }
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
