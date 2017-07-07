using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ratcow.Debugging.Client;

namespace SimpleClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new DebugInterfaceClient("http://127.0.0.1:9001/DebugInterface");

            var names = client.GetVariableNames();

            foreach (var name in names)
            {
                Console.Write(name);
                var value = client.GetVariableValue(name);
                Console.WriteLine($" : {value}");
            }

            Console.WriteLine("\r\n----------------------");
            Console.WriteLine("any key to exit");
            Console.ReadLine();
        }
    }
}
