using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace Ratcow.Debugging.Server
{

    /// <summary>
    /// Simple debug interface (I've created this too many times, now a more generic version.)
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class DebugInterface : IDebugInterface, IDebugConfigurationInterface
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
        ///  NEW - makes adding muliple values a lof more fluid.
        /// </summary>
        public void RegisterInstance(object value, string name = null, bool includeNonPublic = false)
        {
            var type = value?.GetType();
            var level = (includeNonPublic ? BindingFlags.NonPublic | BindingFlags.Public : BindingFlags.Public) | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            var foundProperties = type?.GetProperties(level)?.Select(p => p.Name)?.ToArray();
            if (foundProperties != null)
            {
                RegisterProperties(value, name ?? type.Name, foundProperties);
            }
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
        /// Sets the specific property value
        /// </summary>
        public bool SetVariableValue(string variableName, string json)
        {
            var result = string.Empty;

            if (RegisteredVariables.Any(r => string.Compare(r.RegisteredName, variableName) == 0))
            {
                var variable = RegisteredVariables.First(r => string.Compare(r.RegisteredName, variableName) == 0);
                if (variable.IsDirectReference)
                {
                    return false; //not done this part yet
                }
                else
                {
                    return SetPropertyValue(variable, json);
                }
            }

            return false;
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
        /// Attempts to set a property with a given value.
        /// 
        /// Beware!! There are no validations or checks here and it'll completely overwrite the entire structure!
        /// 
        /// I'll add in a more granular approach shortly.
        /// </summary>
        bool SetPropertyValue(RegisteredVariableItem item, string value)
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
                    try
                    {
                        var objectValue = StringAsInstance(value, pi.PropertyType);
                        if (isStatic)
                        {
                            pi.SetValue(null, objectValue, null);
                        }
                        else
                        {
                            pi.SetValue(item.Reference, objectValue, null);
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }


                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Simple wrapper around deserialising to an instance 
        /// </summary>
        object StringAsInstance(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
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

        [Obsolete]
        public static ServiceHost Start(Action<DebugInterface> startup, bool autoConfig = false, string url = "http://127.0.0.1:9001/DebugInterface")
        {
            return DebugInterfaceFactory.Start<DebugInterface, IDebugInterface>(startup, autoConfig, url);
        }        
    }
}

