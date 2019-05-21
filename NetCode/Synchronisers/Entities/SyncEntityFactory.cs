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
            return new SynchronisableEntity(Descriptor, Descriptor.Constructor.Invoke(), 0);
        }

        public SynchronisableEntity ConstructNewEntity(uint revision)
        {
            return new SynchronisableEntity(Descriptor, Descriptor.Constructor.Invoke(), revision);
        }

        public SynchronisableEntity ConstructForExisting(object obj)
        {
            return new SynchronisableEntity(Descriptor, obj, 0);
        }
    }
}
