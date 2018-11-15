using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCode;

namespace NetcodeTest.Server
{
    public class ServerReport
    {
        [Synchronisable]
        public List<string> Clients = new List<string>();
    }
}
