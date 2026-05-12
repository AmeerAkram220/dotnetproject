using System.Runtime.Serialization;
using System.ServiceModel;

namespace ExchangeOfficeClient.Services;

// ── Data contracts (mirror the service) ─────────────────────────────────────

[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/ExchangeOfficeService")]
public class UserDto
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string Username { get; set; } = string.Empty;
    [DataMember] public string? Error { get; set; }
}

[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/ExchangeOfficeService")]
public class BalanceDto
{
    [DataMember] public string CurrencyCode { get; set; } = string.Empty;
    [DataMember] public decimal Amount { get; set; }
}

[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/ExchangeOfficeService")]
public class ExchangeRateDto
{
    [DataMember] public string Currency { get; set; } = string.Empty;
    [DataMember] public string Code { get; set; } = string.Empty;
    [DataMember] public decimal Mid { get; set; }
    [DataMember] public DateTime EffectiveDate { get; set; }
    [DataMember] public string? Error { get; set; }
}

[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/ExchangeOfficeService")]
public class TransactionDto
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string FromCurrency { get; set; } = string.Empty;
    [DataMember] public string ToCurrency { get; set; } = string.Empty;
    [DataMember] public decimal FromAmount { get; set; }
    [DataMember] public decimal ToAmount { get; set; }
    [DataMember] public decimal Rate { get; set; }
    [DataMember] public DateTime Date { get; set; }
    [DataMember] public string? Error { get; set; }
}

[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/ExchangeOfficeService")]
public class OperationResult
{
    [DataMember] public bool Success { get; set; }
    [DataMember] public string? Error { get; set; }
}

// ── Service contracts ────────────────────────────────────────────────────────

[ServiceContract]
public interface IAccountService
{
    [OperationContract] UserDto Register(string username, string password);
    [OperationContract] UserDto Login(string username, string password);
    [OperationContract] OperationResult TopUpBalance(int userId, decimal amount);
    [OperationContract] List<BalanceDto> GetBalances(int userId);
}

[ServiceContract]
public interface IExchangeRateService
{
    [OperationContract] ExchangeRateDto GetCurrentRate(string currencyCode);
    [OperationContract] List<ExchangeRateDto> GetHistoricalRates(string currencyCode, DateTime from, DateTime to);
    [OperationContract] List<ExchangeRateDto> GetAllCurrentRates();
}

[ServiceContract]
public interface ITransactionService
{
    [OperationContract] TransactionDto BuyCurrency(int userId, string currencyCode, decimal amount);
    [OperationContract] TransactionDto SellCurrency(int userId, string currencyCode, decimal amount);
    [OperationContract] List<TransactionDto> GetTransactionHistory(int userId);
}

// ── Client factory ───────────────────────────────────────────────────────────

public static class ServiceClientFactory
{
    private const string BaseUrl = "http://localhost:5235";

    private static T CreateChannel<T>(string path)
    {
        var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
        var endpoint = new EndpointAddress($"{BaseUrl}/{path}");
        var factory = new ChannelFactory<T>(binding, endpoint);
        return factory.CreateChannel();
    }

    public static IAccountService AccountService() =>
        CreateChannel<IAccountService>("AccountService.svc");

    public static IExchangeRateService ExchangeRateService() =>
        CreateChannel<IExchangeRateService>("ExchangeRateService.svc");

    public static ITransactionService TransactionService() =>
        CreateChannel<ITransactionService>("TransactionService.svc");
}

// ── Session state ────────────────────────────────────────────────────────────

public static class Session
{
    public static int UserId { get; set; }
    public static string Username { get; set; } = string.Empty;
    public static bool IsLoggedIn => UserId > 0;

    public static void Clear()
    {
        UserId = 0;
        Username = string.Empty;
    }
}
