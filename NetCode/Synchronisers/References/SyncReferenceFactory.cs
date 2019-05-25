using System;
using System.Collections.Generic;
using System.Linq;


namespace NetCode.Synchronisers.References
{
    public class SyncReferenceFactory : SynchroniserFactory
    {
        private readonly Type ReferenceType;
        private readonly bool Linked;
        public SyncReferenceFactory(Type refType, SyncFlags flags)
        {
            Linked = (flags & SyncFlags.Linked) != 0;
            ReferenceType = refType;
        }

        public sealed override Synchroniser Construct()
        {
            if (Linked)
            {
                return new SyncLinkedReference(ReferenceType);
            }
            return new SyncReference(ReferenceType);
        }
    }
}