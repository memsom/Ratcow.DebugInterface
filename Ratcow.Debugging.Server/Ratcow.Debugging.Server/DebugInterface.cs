using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Ratcow.Debugging.Server
{

    /// <summary>
    /// Simple debug interface (I've created this too many times, now a more generic version.)
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class DebugInterface : IDebugInterface
    {
        public List<RegisteredVariableItem> RegisteredVariables { get; set; }
        public static Action<DebugInterface> StartupAction { get; set; }

        string[] emptyStringArray = new string[0];

        public DebugInterface()
        {
            RegisteredVariables = new List<RegisteredVariableItem>();

            //this is statically set externally, used by the code to create
            //the service. Not meant to be used outside of that scenario.
            StartupAction?.Invoke(this);

            StartupAction = null; //release it as we don't want to run it again
        }

        public DebugInterface(Action<DebugInterface> startupAction) : this()
        {
            startupAction?.Invoke(this);
        }

        /// <summary>
        /// Registers a direct variable reference
        /// </summary>
        public void RegisterValue<T>(Ref<T> value, string name)
        {
            RegisteredVariables.Add(
                new RegisteredVariableItem
                {
                    Name = name,
                    IsDirectReference = true,
                    Reference = value,
                    RegisteredName = name
                });
        }

        /// <summary>
        /// Registers a property
        /// </summary>
        public void RegisterProperties(object value, string parentName, params string[] properties)
        {
            foreach (var property in properties)
            {
                var registeredName = $"{parentName}.{property}";

                RegisteredVariables.Add(
                    new RegisteredVariableItem
                    {
                        Name = property,
                        IsDirectReference = false,
                        Reference = value,
                        RegisteredName = registeredName
                    });
            }
        }

        /// <summary>
        /// Gets all the registered names
        /// </summary>
        public string[] GetVariableNames()
        {
            var result = RegisteredVariables?.Select(r => r.RegisteredName);
            if (result != null)
            {
                return result.ToArray();
            }

            return emptyStringArray;
        }

        /// <summary>
        /// Converts the registered variable to a string
        /// </summary>
        public string GetVariableValue(string variableName)
        {
            var result = string.Empty;

            if (RegisteredVariables.Any(r => string.Compare(r.RegisteredName, variableName) == 0))
            {

                var variable = RegisteredVariables.First(r => string.Compare(r.RegisteredName, variableName) == 0);
                if (variable.IsDirectReference)
                {
                    result = InstanceAsString(((Ref)variable.Reference).RawValue);
                }
                else
                {
                    result = InstanceAsString(GetPropertyValue(variable));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the specific property value
        /// </summary>
        object GetPropertyValue(RegisteredVariableItem item)
        {
            var type = (item?.Reference is Type) ? item?.Reference as Type : item?.Reference?.GetType();
            if (type != null)
            {
                //TODOL: this is not ideal, but it was what a quick google got me.
                var isStatic = false;
                var pi = type.GetProperty(item.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (pi == null)
                {
                    pi = type.GetProperty(item.Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    isStatic = true;
                }

                if (pi != null)
                {
                    var result = isStatic ? pi.GetValue(null, null) : pi.GetValue(item.Reference, null);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Simple wrapper around serialising an instance 
        /// </summary>
        string InstanceAsString(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        /// <summary>
        /// Simple start-up
        /// </summary>
        public static ServiceHost Start(DebugInterface contract)
        {
            try
            {
                var svcHost = new ServiceHost(contract);
                svcHost.Open();


                return svcHost;
            }
            catch //(Exception ex)
            {
                //TODO - add logging etc
                return null;
            }
        }

        /// <summary>
        /// Simple start-up
        /// 
        /// Passing "autoConfig = true", will create a config less service (with almost no security!!!)
        /// If you have passed autoConfig as true, you can also specify a differnt endpoint URL
        /// </summary>
        public static ServiceHost Start(Action<DebugInterface> startup, bool autoConfig = false, string url = "http://127.0.0.1:9001/DebugInterface")
        {
            try
            {
                DebugInterface.StartupAction = startup;

                if (autoConfig)
                {
                    var contractType = typeof(IDebugInterface);
                    var serviceType = typeof(DebugInterface);
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
            catch //(Exception ex)
            {
                //TODO - add logging etc
                return null;
            }
        }

        private static X509Certificate2 GetCertificate()
        {
            throw new NotImplementedException();
        }
    }
}
