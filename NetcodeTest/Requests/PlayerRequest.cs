using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetCode;

namespace NetcodeTest.Requests
{
    [EnumerateSynchEntity]
    public class PlayerRequest
    {
        public enum RequestType {
            None = 0,
            FireMissile,
            FireMultiMissile,
        }

        [Synchronisable]
        public RequestType Request;

        public PlayerRequest(RequestType request)
        {
            Request = request;
        }

        public PlayerRequest() { }

        
    }
}
