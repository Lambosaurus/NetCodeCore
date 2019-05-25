using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Synchronisers.Entities
{
    internal class SyncDynamicEntityFactory : SynchroniserFactory
    {
        EntityDescriptorCache Cache;
        public SyncDynamicEntityFactory( EntityDescriptorCache cache )
        {
            Cache = cache;
        }

        public override Synchroniser Construct()
        {
            return new SyncDynamicEntity(Cache);
        }
    }
}
