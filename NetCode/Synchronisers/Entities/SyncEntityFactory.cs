using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Synchronisers.Entities
{
    internal class SyncEntityFactory : SynchroniserFactory
    {
        public EntityDescriptor Descriptor { get; private set; }

        public SyncEntityFactory(EntityDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        public override Synchroniser Construct()
        {
            return new SyncEntity(Descriptor, Descriptor.Constructor.Invoke(), 0);
        }

        public SyncEntity ConstructNewEntity(uint revision)
        {
            return new SyncEntity(Descriptor, Descriptor.Constructor.Invoke(), revision);
        }

        public SyncEntity ConstructForExisting(object obj)
        {
            return new SyncEntity(Descriptor, obj, 0);
        }
    }
}
