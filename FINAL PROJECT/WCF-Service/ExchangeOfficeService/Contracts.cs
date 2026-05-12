using CoreWCF;
using System.Runtime.Serialization;

namespace ExchangeOfficeService;

// ── Data Contracts ───────────────────────────────────────────────────────────

[DataContract]
public class UserDto
{
    [DataMember] public int Id { get; set; }
    [DataMember] public string Username { get; set; } = string.Empty;
    [DataMember] public string? Error { get; set; }
}

[DataContract]
public class BalanceDto
{
    [DataMember] public string CurrencyCode { get; set; } = string.Empty;
    [DataMember] public decimal Amount { get; set; }
}

[DataContract]
public class ExchangeRateDto
{
    [DataMember] public string Currency { get; set; } = string.Empty;
    [DataMember] public string Code { get; set; } = string.Empty;
    [DataMember] public decimal Mid { get; set; }
    [DataMember] public DateTime EffectiveDate { get; set; }
    [DataMember] public string? Error { get; set; }
}

[DataContract]
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

[DataContract]
public class OperationResult
{
    [DataMember] public bool Success { get; set; }
    [DataMember] public string? Error { get; set; }
}

// ── Service Contracts ────────────────────────────────────────────────────────

[ServiceContract]
public interface IAccountService
{
    [OperationContract]
    UserDto Register(string username, string password);

    [OperationContract]
    UserDto Login(string username, string password);

    [OperationContract]
    OperationResult TopUpBalance(int userId, decimal amount);

    [OperationContract]
    List<BalanceDto> GetBalances(int userId);
}

[ServiceContract]
public interface IExchangeRateService
{
    [OperationContract]
    ExchangeRateDto GetCurrentRate(string currencyCode);

    [OperationContract]
    List<ExchangeRateDto> GetHistoricalRates(string currencyCode, DateTime from, DateTime to);

    [OperationContract]
    List<ExchangeRateDto> GetAllCurrentRates();
}

[ServiceContract]
public interface ITransactionService
{
    [OperationContract]
    TransactionDto BuyCurrency(int userId, string currencyCode, decimal amount);

    [OperationContract]
    TransactionDto SellCurrency(int userId, string currencyCode, decimal amount);

    [OperationContract]
    List<TransactionDto> GetTransactionHistory(int userId);
}
