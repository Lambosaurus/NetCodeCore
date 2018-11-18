using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Connection;
using NetCode.Util;
using NetCode.SyncPool;

namespace NetCode.Payloads
{
    [EnumeratePayload]
    public class PoolDeletionPayload : Payload
    {
        public override bool AcknowledgementRequired { get { return true; } }
        public override bool ImmediateTransmitRequired { get { return true; } }

        public const int MAX_ENTITY_IDS = byte.MaxValue;
        
        public ushort[] EntityIDs { get; private set; }
        public ushort PoolID { get; protected set; }
        public uint Revision { get; protected set; }


        public PoolDeletionPayload()
        {
        }

        public static PoolDeletionPayload Generate(ushort poolID, uint revision, IEnumerable<ushort> entityIDs)
        {
            //TODO: Potentially remove this once packing is properly abstracted.
            if (entityIDs.Count() > MAX_ENTITY_IDS)
            {
                throw new NetcodeItemcountException(string.Format("May not delete more than {0} entities in one payload", MAX_ENTITY_IDS));
            }

            PoolDeletionPayload payload = new PoolDeletionPayload()
            {
                EntityIDs = entityIDs.ToArray(),
                Revision = revision,
                PoolID = poolID
            };
            payload.AllocateAndWrite();
            return payload;
        }
        
        public override void OnReception(NetworkClient client)
        {
            IncomingSyncPool destination = client.GetSyncPool(PoolID);
            if (destination != null)
            {
                foreach (ushort entityID in EntityIDs)
                {
                    if (destination.HandleExists(entityID))
                    {
                        destination.RemoveEntity(entityID, Revision);
                    }
                }
            }
        }

        public override void OnTimeout(NetworkClient client)
        {
            client.Enqueue(this);
        }

        /*
        private PoolDeletionPayload GenerateRecoveryPayload()
        {
            List<ushort> entityIDs = new List<ushort>();
            foreach (ushort entityID in EntityIDs)
            {
                // Find entities which have since been replaced, and no longer need to be deleted.
                if (SyncPool.GetHandle(entityID) == null)
                {
                    entityIDs.Add(entityID);
                }
            }

            if (entityIDs.Count > 0)
            {
                PoolDeletionPayload payload = new PoolDeletionPayload(SyncPool, Revision, entityIDs);
                connection.Enqueue(payload);
            }
        }
        */

        public override void WriteContent()
        {
            Primitive.WriteUShort(Data, ref DataIndex, PoolID);
            Primitive.WriteUInt(Data, ref DataIndex, Revision);
            Primitive.WriteUShortArray(Data, ref DataIndex, EntityIDs);
        }

        public override void ReadContent()
        {
            PoolID = Primitive.ReadUShort(Data, ref DataIndex);
            Revision = Primitive.ReadUInt(Data, ref DataIndex);
            EntityIDs = Primitive.ReadUShortArray(Data, ref DataIndex);
        }

        public override int ContentSize()
        {
            return sizeof(ushort) + sizeof(uint) + Primitive.ArraySize(EntityIDs.Length, sizeof(ushort));
        }
    }
}
