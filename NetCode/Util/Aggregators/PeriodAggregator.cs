using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.Util.Aggregators
{
    public class PeriodAggregator : IAggregator
    {
        public int PeriodMilliseconds { get; set; }
        public int MinimumCount { get; set; } = 0;
        public int MaximumCount { get; set; } = int.MaxValue;
        
        public long Total { get; private set; }
        public int Count { get { return Values.Count; } }
        public double Average { get { return (Count > 0) ? WindowTotal / (double)Count : 0; } }
        public double PerSecond { get { return WindowTotal * 1000.0 / PeriodMilliseconds; } }
        public long LongAverage { get { return (Count > 0) ? WindowTotal / Count : 0; } }

        private long WindowTotal;
        private List<TimedValue> Values;

        public PeriodAggregator(int period)
        {
            PeriodMilliseconds = period;
            Values = new List<TimedValue>();
            Total = 0;
            WindowTotal = 0;
        }
        
        public void Add(long value, long timestamp)
        {
            Total += value;
            WindowTotal += value;
            Values.Add( new TimedValue()
            {
                Timestamp = timestamp,
                Value = value
            });
        }

        public void Update(long timestamp)
        {
            long culledTotal = 0;
            int end = Values.Count - MinimumCount;
            int k = (Values.Count < MaximumCount) ? 0 : Values.Count - MaximumCount;
            for (k = 0; k < end; k++)
            {
                if (timestamp - Values[k].Timestamp > PeriodMilliseconds)
                {
                    culledTotal += Values[k].Value;
                }
                else
                {
                    break;
                }
            }
            if (k > 0)
            {
                Values.RemoveRange(0, k);
            }
            WindowTotal -= culledTotal;
        }

        protected struct TimedValue {
            public long Timestamp;
            public long Value;
        }

        public void Clear()
        {
            Total = 0;
            WindowTotal = 0;
            Values.Clear();
        }
    }
}
