using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NetCode.SyncEntity;
using NetCode.SyncField;
using NetCode.SyncPool;

namespace NetCode
{
    public class NetDefinitions
    {
        internal SyncEntityGenerator entityGenerator;

        public NetDefinitions(string[] tags = null)
        {
            if (tags == null)
            {
                tags = new string[] { null };
            }
            entityGenerator = new SyncEntityGenerator();
            entityGenerator.LoadEntityTypes(tags);
        }

        uint packetID = 0;
        internal uint GetNextPacketID()
        {
            packetID += 1;
            return packetID;
        }
    }
}
