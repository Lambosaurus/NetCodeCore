using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncPool
{
    public class IncomingSyncPool : SyncPool
    {
        public IncomingSyncPool(NetCodeManager _netcode, ushort poolID) : base(_netcode, poolID)
        {

        }
    }
}
