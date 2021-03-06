﻿using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Synchronisers.Entities;

namespace NetCode
{
    public class NetDefinitions
    {
        internal EntityDescriptorCache entityGenerator;

        public NetDefinitions(string[] tags = null)
        {
            if (tags == null)
            {
                tags = new string[] { null };
            }
            entityGenerator = new EntityDescriptorCache();
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
