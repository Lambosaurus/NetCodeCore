using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Registers a new type of synchronisable entity to the manager.
        /// This must be done before the entity is added to any SyncPools
        /// Objects must be registered in the same order for any client Netcode
        /// </summary>
        /// <param name="sync_type">The synchronisable entity to register</param>
        public void RegisterType(Type syncType)
        {
            entityGenerator.RegisterEntityType(syncType);
        }
    }
}
