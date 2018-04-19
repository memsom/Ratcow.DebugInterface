using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Ratcow.Debugging.Server
{
    public static class DebugInterfaceFactory
    {
        /// <summary>
        /// Simple start-up
        /// 
        /// Passing "autoConfig = true", will create a config less service (with almost no security!!!)
        /// If you have passed autoConfig as true, you can also specify a differnt endpoint URL
        /// </summary>
        public static ServiceHost Start<service, contract>(Action<DebugInterface> startup, bool autoConfig = false, string url = "http://127.0.0.1:9001/DebugInterface")
            where service : DebugInterface
            where contract : IDebugInterface
        {
            try
            {
                DebugInterface.StartupAction = startup;

                if (autoConfig)
                {
                    var contractType = typeof(contract);
                    var serviceType = typeof(service);
                    var baseAddress = new Uri(url);

                    var svcHost = new ServiceHost(serviceType, baseAddress);
                    svcHost.AddServiceEndpoint(contractType, new BasicHttpBinding(), baseAddress);

                    var smb = new ServiceMetadataBehavior()
                    {
                        HttpGetEnabled = true
                    };
                    svcHost.Description.Behaviors.Add(smb);

                    svcHost.Open();

                    return svcHost;
                }
                else
                {
                    var svcHost = new ServiceHost(typeof(DebugInterface));
                    svcHost.Open();


                    return svcHost;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}

