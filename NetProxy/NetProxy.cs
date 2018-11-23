using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using Troschuetz.Random;

namespace NetProxy
{
    public partial class NetProxy
    {
        public bool IsOpen { get; private set; } = false;
        public bool Running { get; private set; }

        public int Latency { get; private set; } = 0;
        public double Loss { get; private set; } = 0;
        public double Deviance { get; private set; } = 0;
        public double SpikeDeviance { get; private set; } = 0;
        public double SpikePeriod { get; private set; } = 0;
        public double SpikeDuration { get; private set; } = 0;
        public int MTU { get; private set; } = 10000; // 548 is a realistic value
        
        private string CommandString = null;
        private bool PendingCommand = false;

        private UdpClient ClientA;
        private UdpClient ClientB;
        private List<Payload> PayloadsA = new List<Payload>();
        private List<Payload> PayloadsB = new List<Payload>();

        private Stopwatch Timer;
        private Random Rand;
        private ChiSquareDistribution Distribution;

        private ulong SpikeTimestamp = 0;
        public bool SpikeOn = false;

        public NetProxy()
        {
            Rand = new Random();
            Distribution = new ChiSquareDistribution();
            Distribution.Alpha = 3;
            Timer = new Stopwatch();
            Timer.Start();
        }

        private void Close()
        {
            if (ClientA != null) { ClientA.Close(); }
            if (ClientB != null) { ClientB.Close(); }
            IsOpen = false;
        }

        private double GetDistributedValue(double mode)
        {
            return Distribution.NextDouble() * mode / 2.5;
        }

        private void Open(int srcA, int dstA, int srcB, int dstB)
        {
            ClientA = new UdpClient(srcA);
            ClientA.Connect(IPAddress.Loopback, dstA);
            ClientB = new UdpClient(srcB);
            ClientB.Connect(IPAddress.Loopback, dstB);
            IsOpen = true;
        }

        private struct Payload
        {
            public ulong Timestamp;
            public byte[] Data;
        }

        private void UpdateSpikes()
        {
            if (SpikeDeviance > 0 && SpikePeriod > 0)
            {
                ulong now = (ulong)Timer.ElapsedMilliseconds;
                if (now > SpikeTimestamp)
                {
                    SpikeOn = !SpikeOn;
                    double r = (Rand.NextDouble() + 0.5);
                    SpikeTimestamp = now + (ulong)( r * r * (SpikeOn ? SpikeDuration : SpikePeriod) * 1000);
                }
            }
        }
        
        private ulong GetDelay()
        {
            ulong delay = (ulong)(Latency / 2);

            if (Deviance > 0)
            {
                delay += (ulong)(GetDistributedValue(Deviance / 2));
            }

            if (SpikeOn && SpikeDeviance > 0)
            {
                delay += (ulong)(GetDistributedValue(SpikeDeviance / 2));
            }

            return delay;
        }

        private bool GetLoss(int packetSize)
        {
            while (packetSize > 0) // Could be handled by binomial distribution..
            {
                packetSize -= MTU;
                if (Rand.NextDouble() < Loss)
                {
                    return true;
                }
            }
            return false;
        }

        private int Recieve(UdpClient client, List<Payload> payloads)
        {
            int recieved = 0;
            ulong now = (ulong)Timer.ElapsedMilliseconds;
            IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
            while (client.Available > 0)
            {
                recieved++;
                byte[] data = client.Receive(ref source);
                if (!GetLoss(data.Length))
                {
                    payloads.Add(new Payload()
                    {
                        Timestamp = now + GetDelay(),
                        Data = data
                    });
                }
            }
            return recieved;
        }

        private List<Payload> Send( UdpClient client, List<Payload> payloads )
        {
            ulong now = (ulong)Timer.ElapsedMilliseconds;
            List<Payload> newPayloads = new List<Payload>();
            foreach (Payload payload in payloads)
            {
                if (payload.Timestamp <= now)
                {
                    client.Send(payload.Data, payload.Data.Length);
                }
                else
                {
                    newPayloads.Add(payload);
                }
            }
            return newPayloads;
        }

        private int Comms()
        {
            int recieved = 0;
            recieved += Recieve(ClientA, PayloadsA);
            recieved += Recieve(ClientB, PayloadsB);
            PayloadsB = Send(ClientA, PayloadsB);
            PayloadsA = Send(ClientB, PayloadsA);
            
            return recieved;
        }
        
        private void ServiceCommand()
        {
            if (PendingCommand)
            {
                string[] items = CommandString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                string Response;

                if (items.Length == 0)
                {
                    Response = "Enter ? for command list.";
                }
                else
                {
                    string commandName = items[0].ToLower();
                    Command command = null;
                    foreach (Command c in Commands)
                    {
                        if (c.Name.ToLower() == commandName)
                        {
                            command = c;
                            break;
                        }
                    }
                    if (command == null)
                    {
                        Response = string.Format("{0} not a known command. Enter ? for a command list.", commandName);
                    }
                    else
                    {
                        if (items.Length != command.Parameters.Length + 1)
                        {
                            Response = string.Format("{0} expects {1} arguments: {2}\n",
                                command.Name,
                                command.Parameters.Length,
                                string.Join(", ", command.Parameters));
                        }
                        else
                        {
                            bool argsOk = true;
                            int[] args = new int[command.Parameters.Length];
                            for (int i = 0; i < args.Length; i ++)
                            {
                                bool success = int.TryParse(items[i + 1], out int n);
                                if (!success)
                                {
                                    argsOk = false;
                                    break;
                                }
                                args[i] = n;
                            }
                            if (argsOk)
                            {
                                Response = command.Function.Invoke(this, args);
                            }
                            else
                            {
                                Response = "All parameters must be integer";
                            }
                        }
                    }
                }
                CommandString = Response;
                PendingCommand = false;
            }
        }

        public string SubmitCommand(string line)
        {
            CommandString = line;
            PendingCommand = true;
            while (PendingCommand)
            {
                Thread.Sleep(10);
            }
            return CommandString;
        }
        
        public void Run()
        {
            Running = true;
            Open(12002, 11003, 12003, 11002);
            while(Running)
            {
                int packets = 0;
                if (IsOpen)
                {
                    packets = Comms();
                }
                UpdateSpikes();
                ServiceCommand();

                if (packets == 0)
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
