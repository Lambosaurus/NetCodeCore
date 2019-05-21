using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.Synchronisers.Entities
{
    internal class EntityDescriptor
    {
        public FieldDescriptor[] Fields { get; protected set; }
        public Func<object> Constructor { get; protected set; }
        public ushort TypeID { get; protected set; }

        public EntityDescriptor(ushort typeID)
        {
            TypeID = typeID;
        }

        public void GenerateFieldDescriptors(Type type, FieldDescriptorCache fieldGenerator)
        {
            Constructor = DelegateGenerator.GenerateConstructor<object>(type);

            List<FieldDescriptor> descriptors = new List<FieldDescriptor>();

            AttributeHelper.ForAllFieldsWithAttribute<SynchronisableAttribute>(type,
               (fieldInfo, attribute) => {
                   descriptors.Add(fieldGenerator.GetFieldDescriptor(fieldInfo, attribute.Flags));
               });
            AttributeHelper.ForAllPropertiesWithAttribute<SynchronisableAttribute>(type,
               (propInfo, attribute) => {
                   descriptors.Add(fieldGenerator.GetFieldDescriptor(propInfo, attribute.Flags));
               });

            if (descriptors.Count >= byte.MaxValue)
            {
                throw new NetcodeItemcountException(string.Format("Type {0} contains more than {1} synchronisable fields.", type.Name, byte.MaxValue));
            }

            Fields = descriptors.ToArray();
        }
    }
}
