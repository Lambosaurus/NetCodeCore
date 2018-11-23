using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetProxy
{
    public partial class NetProxy
    {
        private class Command
        {
            public string Name;
            public string[] Parameters;
            public string Description;
            public Func<NetProxy, int[], String> Function;
        }

        private static Command[] Commands = new Command[]
        {
            new Command()
            {
                Name = "?",
                Parameters = new string[] {},
                Description = "Prints a list of available commands",
                Function = (net, args) => {
                    List<string> lines = new List<string>();
                    foreach (Command c in Commands)
                    {
                        lines.Add( string.Format( "{0}: {1}\n{2}\n", c.Name, string.Join(",", c.Parameters), c.Description ));
                    }
                    return string.Join("\n", lines);
                }
            },
            new Command()
            {
                Name = "Open",
                Parameters = new string[] { "PortA", "DestA", "PortB", "DestB" },
                Description = "Opens PortA and PortB, and connects them to DestA and DestB.",
                Function = (net, args) => {
                    try
                    {
                        net.Open(args[0], args[1], args[2], args[3]);
                    }
                    catch(Exception e)
                    {
                        net.Close();
                        return string.Format("!! {0} !!", e.ToString());
                    }
                    return "Ports opened.";
                }
            },
            new Command()
            {
                Name = "Close",
                Parameters = new string[] { },
                Description = "Closes the open ports.",
                Function = (net, args) => {
                    
                    net.Close();
                    return "Ports closed.";
                }
            },
            new Command()
            {
                Name = "Exit",
                Parameters = new string[] { },
                Description = "Exits the application.",
                Function = (net, args) => {

                    net.Close();
                    net.Running = false;
                    return "Exiting";
                }
            },
            new Command()
            {
                Name = "Latency",
                Parameters = new string[] { "Latency(ms)" },
                Description = "Sets the best case round trip time.",
                Function = (net, args) => {
                    int latency = args[0];
                    if (latency < 0 || latency > 10000) { return "Latency must be between 0 and 10000."; }
                    net.Latency = latency;
                    return string.Format("Latency set to {0}ms", args[0]);
                }
            },
            new Command()
            {
                Name = "Loss",
                Parameters = new string[] { "Probability(%)" },
                Description = "Sets the probability of total packet loss.",
                Function = (net, args) => {
                    int loss = args[0];
                    if (loss < 0 || loss > 100) { return "Loss must be between 0 and 100%."; }
                    net.Loss = loss / 100.0;
                    return string.Format("Loss set to {0}%", loss);
                }
            },
            new Command()
            {
                Name = "Deviance",
                Parameters = new string[] { "Deviance(ms)" },
                Description = "Adds a Chi-Squared distrubtion to packet latency, with k=3. The mode ping will become Latency + Deviance.",
                Function = (net, args) => {
                    int dev = args[0];
                    if (dev < 0 || dev > 1000) { return "Deviance must be between 0 and 1000ms."; }
                    net.Deviance = dev;
                    return string.Format("Deviance set to {0}ms", dev);
                }
            },
            new Command()
            {
                Name = "MTU",
                Parameters = new string[] { "Size(bytes)" },
                Description = "Sets the Maxmuim Transmission unit. Packets will have an additional chance to be per MTU in size. An appropriate MTU is 548.",
                Function = (net, args) => {
                    int mtu = args[0];
                    if (mtu < 10 || mtu > 10000) { return "MTU must be between 10 and 10000 bytes."; }
                    net.MTU = mtu;
                    return string.Format("MTU set to {0} bytes", mtu);
                }
            },
            new Command()
            {
                Name = "Spikes",
                Parameters = new string[] { "Period(s)", "Duration(s)", "Deviance(ms)" },
                Description = "At the approximate period, for the approximate duration, an additional deviance will be imposed on the latency",
                Function = (net, args) => {
                    int period = args[0];
                    int duration = args[1];
                    int deviance = args[2];
                    if (period < 0 || period > 1000) { return "Period must be between 0 and 1000s."; }
                    if (duration < 0 || duration > 1000) { return "Duration must be between 0 and 1000s."; }
                    if (deviance < 0 || deviance > 1000) { return "Deviance must be between 0 and 1000ms."; }

                    net.SpikePeriod = period;
                    net.SpikeDuration = duration;
                    net.SpikeDeviance = deviance;

                    net.SpikeOn = false;
                    net.SpikeTimestamp = (ulong)net.Timer.ElapsedMilliseconds;

                    return string.Format("Spikes configured for approximately {0}ms deviance, for {1}s every {2}s.", deviance, duration, period);
                }
            }
        };
    }
}
