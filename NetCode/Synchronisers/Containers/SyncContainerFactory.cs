using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Synchronisers.Containers
{
    public class SyncContainerListFactory<T> : SynchroniserFactory
    {
        SynchroniserFactory ElementFactory;
        bool DeltaEncoding;
        public SyncContainerListFactory(SynchroniserFactory elementFactory, SyncFlags flags)
        {
            DeltaEncoding = (flags & SyncFlags.NoDeltas) == 0;
            ElementFactory = elementFactory;
        }

        public sealed override Synchroniser Construct()
        {
            return new SyncContainerList<T>(ElementFactory, DeltaEncoding);
        }
    }

    public class SyncContainerArrayFactory<T> : SynchroniserFactory
    {
        SynchroniserFactory ElementFactory;
        bool DeltaEncoding;
        public SyncContainerArrayFactory(SynchroniserFactory elementFactory, SyncFlags flags)
        {
            DeltaEncoding = (flags & SyncFlags.NoDeltas) == 0;
            ElementFactory = elementFactory;
        }

        public sealed override Synchroniser Construct()
        {
            return new SyncContainerArray<T>(ElementFactory, DeltaEncoding);
        }
    }
}
