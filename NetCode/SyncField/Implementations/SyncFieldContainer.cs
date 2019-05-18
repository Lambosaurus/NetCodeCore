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
        
        private readonly SyncFieldFactory ElementFactory;
        private readonly bool DeltaEncoding;
        
        public SyncFieldContainer( SyncFieldFactory elementFactory, bool deltas )
        {
            DeltaEncoding = deltas;
            ElementFactory = elementFactory;
        }

        public sealed override void SetSynchonised(bool sync)
        {
            Synchronised = sync;
            foreach(SynchronisableField element in Elements)
            {
                if (element.Synchronised != sync)
                {
                    element.SetSynchonised(sync);
                }
            }
        }

        protected void SetElementLength(int count)
        {
            if (count > NetBuffer.MaxVWidthValue)
            {
                throw new NetcodeItemcountException(string.Format("There may not be more than {0} items in a Synchronised container", NetBuffer.MaxVWidthValue));
            }

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
                //      Better to replace the list of elements with an Array.
                Elements.RemoveRange(count, Elements.Count - count);
            }
        }
        
        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            ReferencesPending = false;
            ushort elementCount = buffer.ReadVWidth();
            
            if (DeltaEncoding)
            {
                if (context.Revision > Revision)
                {
                    if (elementCount != Elements.Count)
                    {
                        SetElementLength(elementCount);
                        Synchronised = false;
                    }
                }

                int changeCount = buffer.ReadVWidth();
                // If the change count is 0, then we want to default to the read all behavior of non delta lists.
                bool skipHeader = changeCount == elementCount;
                
                for (int i = 0; i < changeCount; i++)
                {
                    int index = skipHeader ? i : buffer.ReadVWidth();

                    if (index < Elements.Count)
                    {
                        SynchronisableField element = Elements[index];
                        element.ReadFromBuffer(buffer, context);
                        ReferencesPending |= element.ReferencesPending;
                        Synchronised &= element.Synchronised;
                    }
                    else
                    {
                        // Elements will be out of range if an out of date revision arrives after after the list is shrunk
                        ElementFactory.SkipFromBuffer(buffer);
                    }
                }
            }
            else
            {
                if (elementCount != Elements.Count)
                {
                    SetElementLength(elementCount);
                    Synchronised = false;
                }

                foreach (SynchronisableField element in Elements)
                {
                    element.ReadFromBuffer(buffer, context);
                    ReferencesPending |= element.ReferencesPending;
                    Synchronised &= element.Synchronised;
                }
            }
        }
        
        public sealed override void SkipFromBuffer(NetBuffer buffer)
        {
            int count = buffer.ReadVWidth();

            if (DeltaEncoding)
            {
                int changeCount = buffer.ReadVWidth();
                for (int i = 0; i < changeCount; i++)
                {
                    buffer.ReadVWidth();
                    ElementFactory.SkipFromBuffer(buffer);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    ElementFactory.SkipFromBuffer(buffer);
                }
            }
        }

        public sealed override bool ContainsRevision(uint revision)
        {
            if (Revision > revision)
            {
                return false;
            }
            else if (Revision == revision)
            {
                return true;
            }
            else if (DeltaEncoding)
            {
                foreach ( SynchronisableField element in Elements)
                {
                    if (element.ContainsRevision(revision))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public sealed override void WriteToBuffer(NetBuffer buffer, SyncContext context)
        {
            if (DeltaEncoding)
            {
                List<ushort> updatedElements = new List<ushort>();

                for (int i = 0; i < Elements.Count; i++)
                {
                    if (Elements[i].ContainsRevision(context.Revision))
                    {
                        updatedElements.Add((ushort)i);
                    }
                }

                buffer.WriteVWidth((ushort)Elements.Count);
                buffer.WriteVWidth((ushort)updatedElements.Count);
                if (updatedElements.Count == Elements.Count)
                {
                    foreach (SynchronisableField element in Elements)
                    {
                        element.WriteToBuffer(buffer, context);
                    }
                }
                else
                {
                    foreach (ushort elementIndex in updatedElements)
                    {
                        buffer.WriteVWidth(elementIndex);
                        Elements[elementIndex].WriteToBuffer(buffer, context);
                    }
                }
            }
            else
            {
                WriteToBuffer(buffer);
            }
        }

        public sealed override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteVWidth((ushort)Elements.Count);
            if (DeltaEncoding)
            {
                buffer.WriteVWidth((ushort)Elements.Count);
            }
            foreach (SynchronisableField element in Elements)
            {
                element.WriteToBuffer(buffer);
            }
        }

        public sealed override int WriteToBufferSize(uint revision)
        {
            if (DeltaEncoding)
            {
                int count = NetBuffer.SizeofVWidth((ushort)Elements.Count);
                int elementHeaderCount = 0;
                ushort changeCount = 0;
                for (int i = 0; i < Elements.Count; i++)
                {
                    SynchronisableField element = Elements[i];
                    if (element.ContainsRevision(revision))
                    {
                        changeCount += 1;
                        elementHeaderCount += NetBuffer.SizeofVWidth((ushort)i);
                        count += element.WriteToBufferSize(revision);
                    }
                }
                count += NetBuffer.SizeofVWidth(changeCount);
                if (changeCount != Elements.Count)
                {
                    count += elementHeaderCount;
                }
                return count;
            }
            else
            {
                return WriteToBufferSize();
            }
        }

        public sealed override int WriteToBufferSize()
        {
            int count = NetBuffer.SizeofVWidth((ushort)Elements.Count);
            
            if (DeltaEncoding)
            {
                count += NetBuffer.SizeofVWidth((ushort)Elements.Count);
            }

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
