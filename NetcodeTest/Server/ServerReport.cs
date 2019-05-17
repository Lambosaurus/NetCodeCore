using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCode;
using NetcodeTest.Entities;

namespace NetcodeTest.Server
{
    [EnumerateSynchEntity]
    public class ServerReport
    {
        
        [Synchronisable]
        public List<string> Clients = new List<string>();
        
        [Synchronisable(SyncFlags.Reference)]
        public List<Ship> Ships = new List<Ship>();

        //ListStreaming
        //[Synchronisable(SyncFlags.Reference)]
        //public List<Entity> Entities = new List<Entity>();
    }
}
