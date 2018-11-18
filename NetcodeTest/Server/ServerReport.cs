using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCode;
using NetcodeTest.Entities;

namespace NetcodeTest.Server
{
    [NetSynchronisableEntity]
    public class ServerReport
    {
        [NetSynchronisable]
        public List<string> Clients = new List<string>();
        
        [NetSynchronisable(SyncFlags.Reference)]
        public List<Ship> Ships = new List<Ship>();
    }
}
