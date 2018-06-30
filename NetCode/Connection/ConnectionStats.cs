using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.Connection
{
    public class ConnectionStats
    {
        public int Latency { get; private set; } = 0;
        public double PacketLoss { get; private set; } = 0.0;
        public int SentBytesPerSecond { get { return (sentBytesOverPeriod * 1000) / AveragingPeriodMilliseconds; } }
        public int RecievedBytesPerSecond { get { return (recievedBytesOverPeriod * 1000) / AveragingPeriodMilliseconds; } }
        public double SentPacketsPerSecond { get { return (sentPacketSizes.Count * 1000.0) / AveragingPeriodMilliseconds; } }
        public double RecievedPacketsPerSecond { get { return (recievedPacketSizes.Count * 1000.0) / AveragingPeriodMilliseconds; } }
        public int MillisecondsSinceLastAcknowledgement { get; private set; } = 0;
        
        /// <summary>
        /// The number of milliseconds that averages are taken over for the PerSecond stats, and the Latency and PacketLoss stats.
        /// This is not immediately applied.
        /// </summary>
        public int AveragingPeriodMilliseconds { get; set; } = 1000;
        /// <summary>
        /// The minimum number of packets to consider over for Latency and PacketLoss calculations.
        /// This is not immediately applied.
        /// </summary>
        public int MinimumLatencyAverageCount { get; set; } = 20;

        public long TotalBytesSent { get; private set; } = 0;
        public long TotalBytesRecieved { get; private set; } = 0;
        public int TotalDamagedPackets { get; private set; } = 0;


        private const int LATENCY_TIMEOUT = -1;
        private struct PacketRecord
        {
            public long Timestamp;
            public int Value;
        }

        private int sentBytesOverPeriod = 0;
        private int recievedBytesOverPeriod = 0;
        private List<PacketRecord> sentPacketSizes = new List<PacketRecord>();
        private List<PacketRecord> recievedPacketSizes = new List<PacketRecord>();
        private List<PacketRecord> packetAcknowledgement = new List<PacketRecord>();
        private long lastAcknowledgedTime = 0;

        internal void RecordReceive( int size, long timestamp, bool damaged )
        {
            TotalBytesRecieved += size;
            recievedBytesOverPeriod += size;

            recievedPacketSizes.Add(
                new PacketRecord {
                    Timestamp = timestamp,
                    Value = size
            });

            if (damaged)
            {
                TotalDamagedPackets++;
            }
        }

        internal void RecordSend(int size, long timestamp)
        {
            TotalBytesSent += size;
            sentBytesOverPeriod += size;

            sentPacketSizes.Add(
                new PacketRecord {
                    Timestamp = timestamp,
                    Value = size
            });
        }

        internal void RecordAcknowledgement(int latency, long timestamp)
        {
            lastAcknowledgedTime = timestamp;
            if (latency < 0)
            {
                throw new NetcodeOverloadedException("Packet latency should never be less than 0.");
            }
            packetAcknowledgement.Add(
                new PacketRecord
                {
                    Timestamp = timestamp,
                    Value = latency
                });
            
        }

        internal void RecordTimeout(long timestamp)
        {
            packetAcknowledgement.Add(
                new PacketRecord
                {
                    Timestamp = timestamp,
                    Value = LATENCY_TIMEOUT
                });
        }

        internal void Update(long timestamp)
        {
            sentBytesOverPeriod -= RemoveOldRecords(sentPacketSizes, timestamp);
            recievedBytesOverPeriod -= RemoveOldRecords(recievedPacketSizes, timestamp);
            
            int culledRecords;
            for (culledRecords = 0; culledRecords < (packetAcknowledgement.Count - MinimumLatencyAverageCount); culledRecords++)
            {
                // go through until either all records are within AveragingPeriodMilliseconds
                // or there are only MinimumLatencyAverageCount records remaining
                PacketRecord record = packetAcknowledgement[culledRecords];
                if (timestamp - record.Timestamp <= AveragingPeriodMilliseconds)
                {
                    break;
                }
            }
            packetAcknowledgement.RemoveRange(0, culledRecords);

            int latencySum = 0;
            int acknowledged = 0;
            int total = packetAcknowledgement.Count;
            foreach ( PacketRecord record in packetAcknowledgement)
            {
                // Average values for the remaining packets.
                if (record.Value != LATENCY_TIMEOUT)
                {
                    latencySum += record.Value;
                    acknowledged++;
                }
            }
            Latency = (acknowledged > 0) ? (latencySum / acknowledged) : 0;
            PacketLoss = (total > 0) ? (1.0 - ((float)acknowledged / total)) : 0.0;

            MillisecondsSinceLastAcknowledgement = (int)(timestamp - lastAcknowledgedTime);
        }
        
        private int RemoveOldRecords(List<PacketRecord> records, long timestamp)
        {
            // assuming records are ordered by time.
            int culledRecords = 0;
            int culledBytes = 0;
            foreach (PacketRecord record in records)
            {
                // Increment until we hit records which are within our averaging window.
                if (timestamp - record.Timestamp > AveragingPeriodMilliseconds)
                {
                    culledRecords++;
                    culledBytes += record.Value;
                }
                else
                {
                    break;
                }
            }
            if (culledRecords > 0)
            {
                records.RemoveRange(0, culledRecords);
            }
            return culledBytes;
        }

        public void ClearTotals()
        {
            TotalBytesSent = 0;
            TotalBytesRecieved = 0;
            TotalDamagedPackets = 0;
        }
    }
}
