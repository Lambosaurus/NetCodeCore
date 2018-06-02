using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;
using NetCode.SyncField;
using NetCode.SyncPool;

namespace NetCode
{
    
    [Flags]
    public enum SyncFlags { None = 0, HalfPrecisionFloats = 1 };

    public enum PacketType { None = 0, PoolUpdate = 1 };

    public class SynchronisableAttribute : System.Attribute
    {
        public SyncFlags Flags { get; private set; }
        public SynchronisableAttribute(SyncFlags flags = SyncFlags.None)
        {
            Flags = flags;
        }
    }

    public class NetcodeOverloadedException : Exception
    {
        public NetcodeOverloadedException(string message) : base(message) { }
    }


    public class NetCodeManager
    {

        internal SyncFieldGenerator fieldGenerator;
        internal SyncEntityGenerator entityGenerator;

        public NetCodeManager()
        {
            fieldGenerator = new SyncFieldGenerator();
            entityGenerator = new SyncEntityGenerator(fieldGenerator);
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

        /// <summary>
        /// Registers a SynchronisableField implementation to enable custom type parsing
        /// This action must be exactly mirrored on any client Netcode
        /// </summary>
        /// <param name="synchronisableType">The SynchronisableField implementation to register against the following type and flags</param>
        /// <param name="fieldType">The type of field parsed by SynchronisableField</param>
        /// <param name="flags">The field implentation may be registered against SyncFlags.HalfPrecision</param>
        public void RegisterField(Type synchronisableType, Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            fieldGenerator.RegisterFieldType(synchronisableType, fieldType, flags);
        }

        public OutgoingSyncPool GenerateOutgoingPool( ushort poolID )
        {
            OutgoingSyncPool outgoingPool = new OutgoingSyncPool(entityGenerator, poolID);
            return outgoingPool;
        }

        public IncomingSyncPool GenerateIncomingPool(ushort poolID)
        {
            IncomingSyncPool incomingPool = new IncomingSyncPool(entityGenerator, poolID);
            return incomingPool;
        }

    }
}
