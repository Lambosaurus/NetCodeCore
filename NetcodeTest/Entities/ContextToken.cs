using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

using NetcodeTest.Entities;
using NetcodeTest.Events;
using NetcodeTest.Server;

namespace NetcodeTest.Entities
{
    public class ContextToken
    {
        List<Entity> Entities = new List<Entity>();
        List<Event> Events = new List<Event>();

        AsteroidServer Server;

        public ContextToken( AsteroidServer server )
        {
            Server = server;
        }

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

        public void AddEvent(Event evt)
        {
            Events.Add(evt);
        }

        public List<Event> GetEvents()
        {
            if (Events.Count > 0)
            {
                List<Event> tmp = Events;
                Events = new List<Event>();
                return tmp;
            }
            return null;
        }

        public List<Physical> GetEntitiesWithin(Vector2 center, float radius)
        {
            return Server.GetPhysicalsInCircle(center, radius);
        }
    }
}
