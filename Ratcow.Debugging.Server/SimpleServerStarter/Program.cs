using System;

namespace SimpleServerStarter
{
    /// <summary>
    /// Thrown together to quickly bootstrap the service
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var starter = new Starter().Init();

            Console.WriteLine("started - press a key to exit");

            Console.ReadLine();
        }
    }
}
