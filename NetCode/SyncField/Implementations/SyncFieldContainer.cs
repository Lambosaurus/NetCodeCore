using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;
using NetCode.Util;

using System.Reflection;

namespace NetCode.SyncField.Implementations
{   
    public abstract class SyncFieldContainer<T> : SynchronisableField
    {
        protected List<SynchronisableField> Elements = new List<SynchronisableField>();
        private SyncFieldFactory ElementFactory;
        
        public SyncFieldContainer( SyncFieldFactory elementFactory )
        {
            ElementFactory = elementFactory;
        }

        public sealed override void SetSynchonised(bool sync)
        {
            Synchronised = sync;
            foreach(SynchronisableField element in Elements)
            {
                element.SetSynchonised(sync);
            }
        }

        protected void SetElementLength(int count)
        {
            if (count > Elements.Count)
            {
                for (int i = Elements.Count; i < count; i++)
                {
                    Elements.Add(ElementFactory.Construct());
                }
            }
            else
            {
                //TODO: I'd prefer not to delete these, and leave them for later.
                Elements.RemoveRange(count, Elements.Count - count);
            }
        }
        
        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            ReferencesPending = false;
            ushort count = buffer.ReadVWidth();
            if (count != Elements.Count)
            {
                SetElementLength(count);
                Synchronised = false;
            }
            foreach (SynchronisableField element in Elements)
            {
                element.ReadFromBuffer(buffer, context);
                ReferencesPending |= element.ReferencesPending;
                Synchronised &= element.Synchronised;
            }
        }
        
        public sealed override void SkipFromBuffer(NetBuffer buffer)
        {
            int count = buffer.ReadVWidth();
            for (int i = 0; i < count; i++)
            {
                ElementFactory.SkipFromBuffer(buffer);
            }
        }
        
        public sealed override void WriteToBuffer(NetBuffer buffer)
        {
            if (Elements.Count > NetBuffer.MaxVWidthValue)
            {
                throw new NetcodeItemcountException(string.Format("There may not be more than {0} items in a Synchronised container", NetBuffer.MaxVWidthValue));
            }
            buffer.WriteVWidth((ushort)Elements.Count);
            foreach ( SynchronisableField element in Elements )
            {
                element.WriteToBuffer(buffer);
            }
        }

        public sealed override int WriteToBufferSize()
        {
            int count = NetBuffer.SizeofVWidth((ushort)Elements.Count);
            foreach (SynchronisableField element in Elements)
            {
                count += element.WriteToBufferSize();
            }
            return count;
        }

        public sealed override void UpdateReferences(SyncContext context)
        {
            ReferencesPending = false;
            foreach (SynchronisableField element in Elements)
            {
                if (element.ReferencesPending)
                {
                    element.UpdateReferences(context);
                    ReferencesPending |= element.ReferencesPending;
                }
            }
        }
    }
}
