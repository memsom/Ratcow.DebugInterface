using System.ServiceModel;

namespace Ratcow.Debugging.Server
{
    [ServiceContract]
    public interface IDebugInterface
    {
        [OperationContract]
        string[] GetVariableNames();

        [OperationContract]
        string GetVariableValue(string variableName);

        [OperationContract]
        bool SetVariableValue(string variableName, string json);
    }
}
