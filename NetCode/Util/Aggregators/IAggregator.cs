using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
