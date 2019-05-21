using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Util
{
    internal class EventMarker
    {
        private long Timestamp;
        private bool Set;

        public EventMarker()
        {
            Clear();
        }

        public bool MarkedInPast( long period )
        {
            return (Set) ? ( Timestamp > NetTime.Now() - period) : false;
        }
        
        public void Mark()
        {
            Timestamp = NetTime.Now();
            Set = true;
        }

        public void Clear()
        {
            Set = false;
        }
    }
}
