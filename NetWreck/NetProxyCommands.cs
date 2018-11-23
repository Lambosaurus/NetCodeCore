using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetWreck
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
                Name = "Commands",
                Parameters = new string[] {},
                Description = "Prints a list of available commands",
                Function = (net, args) => {
                    List<string> names = new List<string>();
                    foreach (Command c in Commands)
                    {
                        names.Add( c.Name );
                    }
                    return string.Join(",\n", names);
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
                Name = "Ping",
                Parameters = new string[] { "RoundTripTime(ms)" },
                Description = "Sets the normal packet latency.",
                Function = (net, args) => {
                    net.Latency = args[0];
                    return string.Format("Latency set to {0}ms", args[0]);
                }
            },
            new Command()
            {
                Name = "Loss",
                Parameters = new string[] { "LossProbability(%)" },
                Description = "Sets the packet loss probability.",
                Function = (net, args) => {
                    int loss = args[0];
                    if (loss < 0 || loss > 100) { return "Lost must be between 0 and 100%."; }
                    net.Loss = loss / 100.0;
                    return string.Format("Loss set to {0}%", loss);
                }
            }
        };
    }
}
