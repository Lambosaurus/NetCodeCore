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
            return new SynchronisableEntity(Descriptor);
        }

        public SynchronisableEntity ConstructNewEntity(uint revision)
        {
            return new SynchronisableEntity(Descriptor, null, revision);
        }

        public SynchronisableEntity ConstructForExisting(object obj)
        {
            return new SynchronisableEntity(Descriptor, obj, 0);
        }
    }
}
