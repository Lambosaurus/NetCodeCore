using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Synchronisers
{
    public abstract class SynchroniserFactory
    {
        Synchroniser StaticField;
        public abstract Synchroniser Construct();

        public void SkipFromBuffer(NetBuffer buffer)
        {
            if (StaticField == null)
            {
                StaticField = Construct();
            }
            StaticField.SkipFromBuffer(buffer);
        }
    }
}
