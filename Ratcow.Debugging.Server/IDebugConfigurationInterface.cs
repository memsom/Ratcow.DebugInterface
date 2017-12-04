using System.Collections.Generic;
using System.ServiceModel;

namespace Ratcow.Debugging.Server
{
    [ServiceContract]
    public interface IDebugConfigurationInterface: IDebugInterface
    {
        List<RegisteredVariableItem> RegisteredVariables { get; set; }

        [OperationContract]
        void RegisterProperties(object value, string parentName, params string[] properties);

        [OperationContract]
        void RegisterValue<T>(Ref<T> value, string name);
    }
}