namespace ExchangeOfficeService;

public class AccountService : IAccountService
{
    public UserDto Register(string username, string password) =>
        new() { Error = "Not implemented yet." };

    public UserDto Login(string username, string password) =>
        new() { Error = "Not implemented yet." };

    public OperationResult TopUpBalance(int userId, decimal amount) =>
        new() { Success = false, Error = "Not implemented yet." };

    public List<BalanceDto> GetBalances(int userId) => [];
}

public class ExchangeRateService : IExchangeRateService
{
    public ExchangeRateDto GetCurrentRate(string currencyCode) =>
        new() { Error = "Not implemented yet." };

    public List<ExchangeRateDto> GetHistoricalRates(string currencyCode, DateTime from, DateTime to) => [];
}

public class TransactionService : ITransactionService
{
    public TransactionDto BuyCurrency(int userId, string currencyCode, decimal amount) =>
        new() { Error = "Not implemented yet." };

    public TransactionDto SellCurrency(int userId, string currencyCode, decimal amount) =>
        new() { Error = "Not implemented yet." };

    public List<TransactionDto> GetTransactionHistory(int userId) => [];
}
