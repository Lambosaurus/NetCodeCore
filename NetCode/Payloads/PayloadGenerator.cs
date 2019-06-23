using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.Payloads
{
    public static class PayloadGenerator
    {
        public const int MaxPayloadID = byte.MaxValue;
        public static List<Func<Payload>> PayloadConstructors = new List<Func<Payload>>();
        public static Dictionary<RuntimeTypeHandle, byte> PayloadTypeIDs = new Dictionary<RuntimeTypeHandle, byte>();

        static PayloadGenerator()
        {
            List<Type> PayloadTypes = new List<Type>();

            AttributeHelper.ForAllTypesWithAttribute<EnumeratePayloadAttribute>(
                (type, attribute) => {  PayloadTypes.Add(type); }
            );

            PayloadTypes.Sort((a, b) => (a.Name.CompareTo(b.Name)));

            for (int i = 0; i < PayloadTypes.Count; i++)
            {
                Type type = PayloadTypes[i];
                PayloadConstructors.Add(DelegateGenerator.GenerateConstructor<Payload>(type));
                PayloadTypeIDs[type.TypeHandle] = (byte)i;
            }
        }

        public static Payload GeneratePayload(byte payloadID)
        {
            if (payloadID < PayloadConstructors.Count)
            {
                return PayloadConstructors[payloadID].Invoke();
            }
            return null;
        }

        public static byte GetPayloadID(RuntimeTypeHandle typeHandle)
        {
            return PayloadTypeIDs[typeHandle];
        }
    }
}
