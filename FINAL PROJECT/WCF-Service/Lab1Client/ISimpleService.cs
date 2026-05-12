using System.ServiceModel;

namespace Lab1Client;

[ServiceContract]
public interface ISimpleService
{
    [OperationContract]
    string GetMessage();
}
