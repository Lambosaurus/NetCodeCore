using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NetCode.Connection
{
    public class ConnectionStats
    {
        // TODO:
        public int Latency = 0;
        public double PacketLoss = 1.0;

        // I think these all work
        public int SentBytesPerSecond { get { return (sentBytesOverPeriod * 1000) / AveragingPeriodMilliseconds; } }
        public int RecievedBytesPerSecond { get { return (recievedBytesOverPeriod * 1000) / AveragingPeriodMilliseconds; } }
        public double SendPacketsPerSecond { get { return (sentPackets.Count * 1000.0) / AveragingPeriodMilliseconds; } }
        public double RecievedPacketsPerSecond { get { return (recievedPackets.Count * 1000.0) / AveragingPeriodMilliseconds; } }

        public int AveragingPeriodMilliseconds { get; set; } = 5000;
        
        public long TotalBytesSent { get; private set; } = 0;
        public long TotalBytesRecieved { get; private set; } = 0;
        public int TotalDamagedPackets { get; private set; } = 0;
        
        private int sentBytesOverPeriod = 0;
        private int recievedBytesOverPeriod = 0;
        private List<PacketRecord> sentPackets = new List<PacketRecord>();
        private List<PacketRecord> recievedPackets = new List<PacketRecord>();
        
        
        internal void RecordReceive( int size, long timestamp, bool damaged )
        {
            TotalBytesRecieved += size;
            recievedBytesOverPeriod += size;

            recievedPackets.Add(
                new PacketRecord {
                Timestamp = timestamp,
                Size = size
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

            sentPackets.Add(
                new PacketRecord
                {
                    Timestamp = timestamp,
                    Size = size
                });
        }

        internal void RecordAcknowledgement(int latency, long timestamp)
        {
            throw new NotImplementedException();
        }

        internal void RecordUnacknowledgement(long timestamp)
        {
            throw new NotImplementedException();
        }

        internal void Update(long timestamp)
        {
            sentBytesOverPeriod -= RemoveOldRecords(sentPackets, timestamp);
            recievedBytesOverPeriod -= RemoveOldRecords(recievedPackets, timestamp);
        }
        
        private int RemoveOldRecords(List<PacketRecord> records, long timestamp)
        {
            // assuming records are ordered by time.
            int culledPackets = 0;
            int culledBytes = 0;
            foreach (PacketRecord record in records)
            {
                if (timestamp - record.Timestamp > AveragingPeriodMilliseconds)
                {
                    culledPackets++;
                    culledBytes += record.Size;
                }
                else
                {
                    break;
                }
            }
            if (culledPackets > 0)
            {
                sentPackets.RemoveRange(0, culledPackets);
            }
            return culledBytes;
        }

        public void ClearTotals()
        {
            TotalBytesSent = 0;
            TotalBytesRecieved = 0;
            TotalDamagedPackets = 0;
        }

        private struct PacketRecord
        {
            public long Timestamp;
            public int Size;
        }
    }
}
