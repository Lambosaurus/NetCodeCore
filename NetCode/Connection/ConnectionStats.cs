using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util.Aggregators;

namespace NetCode.Connection
{
    public class ConnectionStats
    {
        public int Latency { get { return (int)LatencyAggregator.Average; } }
        public double PacketLoss { get { return LossAggregator.Average; } }
        public long NetTimeOffset { get { return NetTimeOffsetAggregator.LongAverage; } }

        public int TotalDamagedPackets { get; protected set; } = 0;
        public int MillisecondsSinceAcknowledgement { get; protected set; } = 0;
        
        /// <summary>
        /// The number of milliseconds that averages are taken over for the PerSecond stats
        /// This is not immediately applied.
        /// </summary>
        public int ByteAggregationPeriodMilliseconds
        {
            get { return byteAveragingPeriodMilliseconds; }
            set
            {
                byteAveragingPeriodMilliseconds = value;
                BytesSentAggregator.PeriodMilliseconds = value;
                BytesRecievedAggregator.PeriodMilliseconds = value;
            }
        }
        private int byteAveragingPeriodMilliseconds = 1000;
        
        // These aggregators are exposed to the client for things.
        public IAggregator BytesSent { get { return BytesSentAggregator; } }
        public IAggregator BytesRecieved { get { return BytesRecievedAggregator; } }

        private PeriodAggregator BytesSentAggregator;
        private PeriodAggregator BytesRecievedAggregator;
        

        // The following aggregators use these constant parameters. This is because the client uses these stats for stuff.
        private const int TimeAggregationMinimumCount = 20;
        private const int TimeAggregationPeriodMilliseconds = 2000;

        private PeriodAggregator LatencyAggregator;
        private PeriodAggregator LossAggregator;
        private PeriodAggregator NetTimeOffsetAggregator;
        
        private long lastAcknowledgedTime = 0;


        internal ConnectionStats()
        {
            BytesSentAggregator = new PeriodAggregator(ByteAggregationPeriodMilliseconds);
            BytesRecievedAggregator = new PeriodAggregator(ByteAggregationPeriodMilliseconds);

            LossAggregator = new PeriodAggregator(TimeAggregationPeriodMilliseconds);
            LatencyAggregator = new PeriodAggregator(TimeAggregationPeriodMilliseconds);
            NetTimeOffsetAggregator = new PeriodAggregator(TimeAggregationPeriodMilliseconds);
            LossAggregator.MinimumCount = TimeAggregationMinimumCount;
            LatencyAggregator.MinimumCount = TimeAggregationMinimumCount;
            NetTimeOffsetAggregator.MinimumCount = TimeAggregationMinimumCount;
        }

        internal void RecordReceive( int size, long timestamp, bool damaged )
        {
            BytesRecievedAggregator.Add(size, timestamp);
            if (damaged) { TotalDamagedPackets++; }
        }

        internal void RecordNetTimeOffset(long offset, long timestamp)
        {
            NetTimeOffsetAggregator.Add(offset, timestamp);
        }

        internal void RecordSend(int size, long timestamp)
        {
            BytesSentAggregator.Add(size, timestamp);
        }

        internal void RecordAcknowledgement(int latency, long timestamp)
        {
            lastAcknowledgedTime = timestamp;
            LatencyAggregator.Add(latency, timestamp);
            LossAggregator.Add(0, timestamp);
        }

        internal void RecordTimeout(long timestamp)
        {
            LossAggregator.Add(1, timestamp);
        }

        internal void Update(long timestamp)
        {
            BytesSentAggregator.Update(timestamp);
            BytesRecievedAggregator.Update(timestamp);
            LatencyAggregator.Update(timestamp);
            LossAggregator.Update(timestamp);
            NetTimeOffsetAggregator.Update(timestamp);

            MillisecondsSinceAcknowledgement = (int)(timestamp - lastAcknowledgedTime);
        }

        public void ClearTotals()
        {
            BytesSentAggregator.Clear();
            BytesRecievedAggregator.Clear();
            TotalDamagedPackets = 0;
        }
    }
}
