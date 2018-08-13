using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetcodeTest.Entities;

namespace NetcodeTest.Entities
{
    public class ContextToken
    {
        List<Entity> Entities = new List<Entity>();
        
        public void AddEntity(Entity entity)
        {
            Entities.Add(entity);
        }

        public List<Entity> GetEntities()
        {
            if (Entities.Count > 0)
            {
                List<Entity> tmp = Entities;
                Entities = new List<Entity>();
                return tmp;
            }
            return null;
        }

    }
}
