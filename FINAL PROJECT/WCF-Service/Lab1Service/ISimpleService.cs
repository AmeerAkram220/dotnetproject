using CoreWCF;

namespace Lab1Service;

[ServiceContract]
public interface ISimpleService
{
    [OperationContract]
    string GetMessage();
}
