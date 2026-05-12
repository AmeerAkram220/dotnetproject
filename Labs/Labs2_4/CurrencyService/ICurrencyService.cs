using CoreWCF;
using System.Runtime.Serialization;

namespace CurrencyService;

[ServiceContract]
public interface ICurrencyService
{
    [OperationContract]
    ExchangeRateResult GetExchangeRate(string currencyCode);
}

[DataContract]
public class ExchangeRateResult
{
    [DataMember]
    public string Currency { get; set; } = string.Empty;

    [DataMember]
    public string Code { get; set; } = string.Empty;

    [DataMember]
    public decimal Mid { get; set; }

    [DataMember]
    public string? Error { get; set; }
}
