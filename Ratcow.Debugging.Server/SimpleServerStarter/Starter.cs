using Ratcow.Debugging.Server;
using Ratcow.Debugging.Server.Tests;
using System.ServiceModel;

namespace SimpleServerStarter
{
    class Starter
    {
        ServiceHost serviceHost;
        TestClass c;

        public Starter()
        {
            Interface = new DebugInterface();

            c = new TestClass()
            {
                I = 10,
                S = "A test message"
            };

            Interface.RegisterProperties(c, "c", "I", "S");
        }

        public DebugInterface Interface { get; private set; }

        public Starter Init()
        {
            serviceHost = DebugInterface.Start(Interface);

            return this;
        }
    }
}
