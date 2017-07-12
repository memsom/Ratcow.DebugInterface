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
            c = new TestClass()
            {
                I = 10,
                S = "A test message"
            };
        }

        public Starter Init()
        {
            serviceHost = DebugInterface.Start(
                (Interface) =>
                {                   
                    Interface.RegisterProperties(
                        c,         //Instance
                        "c",       //Registered name
                        "I", "S"   //Registered properties
                    );
                }, 
                true               //create a dynamic config (with BasicHttpBinding!!!)               
            );

            return this;
        }

        public Starter Init2()
        {
            serviceHost = DebugInterface.Start(
                (Interface) =>
                {
                    Interface.RegisterProperties(
                        c,         //Instance
                        "c",       //Registered name
                        "I", "S"   //Registered properties
                    );
                },
                true,                             //create a dynamic config (with BasicHttpBinding!!!)
                "http://127.0.0.1:9001/SuperTest" //use this URL as the base
            );

            return this;
        }
    }
}
