using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.Synchronisers.Values
{
    public class SyncValueFactory : SynchroniserFactory
    {
        Func<Synchroniser> Constructor;
        public SyncValueFactory(Func<Synchroniser> constructor)
        {
            Constructor = constructor;
        }

        public SyncValueFactory(Type syncFieldType)
        {
            Constructor = DelegateGenerator.GenerateConstructor<Synchroniser>(syncFieldType);
        }

        public sealed override Synchroniser Construct()
        {
            return Constructor.Invoke();
        }
    }
}
