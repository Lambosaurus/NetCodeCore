using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace NetWreck
{
    public partial class NetProxy
    {
        private UdpClient ClientA;
        private UdpClient ClientB;
        private Random Rand;
        private Stopwatch Timer;

        public bool IsOpen { get; private set; } = false;

        public int Latency { get; private set; } = 0;
        public double Loss { get; private set; } = 0;

        public NetProxy()
        {
            Rand = new Random();
            Timer = new Stopwatch();
            Timer.Start();
        }

        private void Close()
        {
            if (ClientA != null) { ClientA.Close(); }
            if (ClientB != null) { ClientB.Close(); }
            IsOpen = false;
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

        private List<Payload> PayloadsA = new List<Payload>();
        private List<Payload> PayloadsB = new List<Payload>();

        private ulong GetDelay()
        {
            return (ulong)(Latency / 2);
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
                if (Rand.NextDouble() > Loss)
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

        private string CommandString = null;
        bool PendingCommand = false;

        private void ServiceCommand()
        {
            if (PendingCommand)
            {
                string[] items = CommandString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                string Response;

                if (items.Length == 0)
                {
                    Response = "Type Help for command list.";
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
                        Response = string.Format("{0} not a known command. Enter Commands for a command list.", commandName);
                    }
                    else
                    {
                        if (items.Length != command.Parameters.Length + 1)
                        {
                            Response = string.Format("{0} expects {1} arguments: {2}",
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

        public bool Running { get; private set; }
        
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
                
                ServiceCommand();

                if (packets == 0)
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
