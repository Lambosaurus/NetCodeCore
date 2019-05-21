using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode
{
    [Serializable]
    public class NetcodeItemcountException : Exception
    {
        public NetcodeItemcountException(string message) : base(message) { }
    }

    [Serializable]
    public class NetcodeGenerationException : Exception
    {
        public NetcodeGenerationException(string message) : base(message) { }
    }
}
