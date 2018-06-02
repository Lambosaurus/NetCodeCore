using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncPool;

namespace NetCode.Packet
{
    public class Packet
    {
    }

    public abstract class Datagram
    {
        enum Type { None, PoolRevision };

        internal byte[] data;
    }

    public class PoolRevisionDatagram : Datagram
    {
        ushort PoolID;
        uint Revision;

        public PoolRevisionDatagram(ushort poolID, uint revision)
        {
            PoolID = poolID;
            Revision = revision;
        }
        
    }
}
