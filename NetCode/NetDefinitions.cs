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

        public NetDefinitions()
        {
            entityGenerator = new SyncEntityGenerator();
        }
        
        uint packetID = 0;
        internal uint GetNextPacketID()
        {
            packetID += 1;
            return packetID;
        }

        /// <summary>
        /// Registers types to the NetDefinitions so that they may be used in any SyncPools this NetDefinitons is based on.
        /// Any two linked SyncPools MUST have the same NetDefinitons. Any inconsistancy in class names, number, and load order WILL cause errors.
        /// </summary>
        /// <param name="tags">A list of tags to match NetSynchronisableEntityAttribtes by.</param>
        public void LoadEntityTypes(string[] tags = null)
        {
            if (tags == null)
            {
                tags = new string[] { null };
            }
            else
            {
                string[] nTags = new string[tags.Length + 1];
                nTags[0] = null;
                tags.CopyTo(nTags, 1);
                tags = nTags;
            }

            entityGenerator.LoadEntityTypes(tags);
        }
    }
}
