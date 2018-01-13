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
        public SyncFlags flags { get; private set; }
        public SynchronisableAttribute(SyncFlags _flags = SyncFlags.None)
        {
            flags = _flags;
        }
    }

    public class NetcodeOverloadedException : Exception
    {
        public NetcodeOverloadedException(string message) : base(message) { }
    }


    public class NetCodeManager
    {
        Dictionary<RuntimeTypeHandle, SyncEntityDescriptor> entityDescriptors = new Dictionary<RuntimeTypeHandle, SyncEntityDescriptor>();
        SyncFieldgenerator fieldGenerator = new SyncFieldgenerator();

        ushort last_typeid = 0;
        private ushort GetNewTypeID()
        {
            if (last_typeid == ushort.MaxValue) { throw new NetcodeOverloadedException(string.Format("There may not be more than {0} unique types registered.",ushort.MaxValue)); }
            return last_typeid++;
        }

        /// <summary>
        /// Registers a new type of synchronisable entity to the manager.
        /// This must be done before the entity is added to any SyncPools
        /// Objects must be registered in the same order for any client Netcode
        /// </summary>
        /// <param name="sync_type">The synchronisable entity to register</param>
        public void RegisterType(Type sync_type)
        {
            entityDescriptors[sync_type.TypeHandle] = new SyncEntityDescriptor(fieldGenerator, sync_type, GetNewTypeID());
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
            if (synchronisableType.BaseType != typeof(SynchronisableField))
            {
                throw new ArgumentException(string.Format("{0} must inherit from {1}", synchronisableType.Name, typeof(SynchronisableField).Name));
            }

            fieldGenerator.RegisterSynchronisableFieldType(synchronisableType, fieldType, flags);
        }

        internal SyncEntityDescriptor GetDescriptor(RuntimeTypeHandle typeHandle)
        {
            return entityDescriptors[typeHandle];
        }

        
        public OutgoingSyncPool GenerateOutgoingPool( ushort poolID )
        {
            OutgoingSyncPool outgoingPool = new OutgoingSyncPool(this, poolID);
            return outgoingPool;
        }

        public IncomingSyncPool GenerateIncomingPool(ushort poolID)
        {
            IncomingSyncPool incomingPool = new IncomingSyncPool(this, poolID);
            return incomingPool;
        }

    }
}
