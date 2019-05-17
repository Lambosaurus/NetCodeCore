using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncField.Implementations;
using NetCode.Util;

namespace NetCode.SyncField
{
    public abstract class SyncFieldFactory
    {
        SynchronisableField StaticField;
        public abstract SynchronisableField Construct();

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
