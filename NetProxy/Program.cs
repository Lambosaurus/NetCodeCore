using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;



namespace NetProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            NetProxy connection = new NetProxy();

            new Thread(() =>
            {
                connection.Run();
            }).Start();

            Thread.Sleep(100);

            while (connection.Running)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                string response = connection.SubmitCommand(line);
                Console.WriteLine(response);
            }
        }
    }
}
