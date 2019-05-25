using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Util.Aggregators
{
    public interface IAggregator
    {
        long Total { get; }
        int Count { get; }
        double Average { get; }
        double PerSecond { get; }
    }
}
