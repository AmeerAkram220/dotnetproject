using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace ExchangeOfficeService;

// ── Account Service ──────────────────────────────────────────────────────────

public class AccountService : IAccountService
{
    public UserDto Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new UserDto { Error = "Username and password are required." };

        if (username.Trim().Length < 3)
            return new UserDto { Error = "Username must be at least 3 characters." };

        if (password.Length < 6)
            return new UserDto { Error = "Password must be at least 6 characters." };

        if (DataStore.Users.Values.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return new UserDto { Error = "Username already taken." };

        var user = new UserRecord
        {
            Id = DataStore.NextUserId(),
            Username = username,
            PasswordHash = PasswordHelper.Hash(password)
        };

        DataStore.Users[user.Id] = user;
        DataStore.Balances[user.Id] = new ConcurrentDictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        DataStore.Balances[user.Id]["PLN"] = 0m;

        return new UserDto { Id = user.Id, Username = user.Username };
    }

    public UserDto Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new UserDto { Error = "Username and password are required." };

        var user = DataStore.Users.Values
            .FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user is null || !PasswordHelper.Verify(password, user.PasswordHash))
            return new UserDto { Error = "Invalid username or password." };

        return new UserDto { Id = user.Id, Username = user.Username };
    }

    public OperationResult TopUpBalance(int userId, decimal amount)
    {
        if (amount <= 0)
            return new OperationResult { Success = false, Error = "Amount must be greater than zero." };

        if (!DataStore.Balances.TryGetValue(userId, out var balances))
            return new OperationResult { Success = false, Error = "User not found." };

        balances.AddOrUpdate("PLN", amount, (_, old) => old + amount);
        return new OperationResult { Success = true };
    }

    public List<BalanceDto> GetBalances(int userId)
    {
        if (!DataStore.Balances.TryGetValue(userId, out var balances))
            return [];

        return balances
            .Where(b => b.Value > 0)
            .Select(b => new BalanceDto { CurrencyCode = b.Key.ToUpper(), Amount = b.Value })
            .OrderBy(b => b.CurrencyCode)
            .ToList();
    }

    public OperationResult ChangePassword(int userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return new OperationResult { Success = false, Error = "New password must be at least 6 characters." };

        if (!DataStore.Users.TryGetValue(userId, out var user))
            return new OperationResult { Success = false, Error = "User not found." };

        if (!PasswordHelper.Verify(currentPassword, user.PasswordHash))
            return new OperationResult { Success = false, Error = "Current password is incorrect." };

        user.PasswordHash = PasswordHelper.Hash(newPassword);
        return new OperationResult { Success = true };
    }
}

// ── Exchange Rate Service ────────────────────────────────────────────────────

public class ExchangeRateService : IExchangeRateService
{
    public ExchangeRateDto GetCurrentRate(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return new ExchangeRateDto { Error = "Currency code is required." };

        try
        {
            var resp = NbpClient.GetCurrentRateAsync(currencyCode).GetAwaiter().GetResult();

            if (resp?.Rates is null || resp.Rates.Length == 0)
                return new ExchangeRateDto { Error = $"No data for '{currencyCode.ToUpper()}'." };

            return new ExchangeRateDto
            {
                Currency = resp.Currency,
                Code = resp.Code,
                Mid = resp.Rates[0].Mid,
                EffectiveDate = resp.Rates[0].EffectiveDate
            };
        }
        catch (HttpRequestException)
        {
            return new ExchangeRateDto { Error = $"Currency '{currencyCode.ToUpper()}' not found or NBP API unavailable." };
        }
        catch (TaskCanceledException)
        {
            return new ExchangeRateDto { Error = "NBP API request timed out." };
        }
    }

    public List<ExchangeRateDto> GetHistoricalRates(string currencyCode, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) return [];

        try
        {
            var resp = NbpClient.GetHistoricalRatesAsync(currencyCode, from, to).GetAwaiter().GetResult();
            if (resp?.Rates is null) return [];

            return resp.Rates.Select(r => new ExchangeRateDto
            {
                Currency = resp.Currency,
                Code = resp.Code,
                Mid = r.Mid,
                EffectiveDate = r.EffectiveDate
            }).ToList();
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (TaskCanceledException)
        {
            return [];
        }
    }

    public List<ExchangeRateDto> GetAllCurrentRates()
    {
        try
        {
            var tables = NbpClient.GetAllCurrentRatesAsync().GetAwaiter().GetResult();
            if (tables is null || tables.Length == 0) return [];

            var table = tables[0];
            return table.Rates.Select(r => new ExchangeRateDto
            {
                Currency = r.Currency,
                Code = r.Code,
                Mid = r.Mid,
                EffectiveDate = table.EffectiveDate
            }).ToList();
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (TaskCanceledException)
        {
            return [];
        }
    }
}

// ── Transaction Service ──────────────────────────────────────────────────────

public class TransactionService : ITransactionService
{
    private readonly ExchangeRateService _rateService = new();

    public TransactionDto BuyCurrency(int userId, string currencyCode, decimal amount)
    {
        if (amount <= 0)
            return new TransactionDto { Error = "Amount must be greater than zero." };

        if (!DataStore.Balances.TryGetValue(userId, out var balances))
            return new TransactionDto { Error = "User not found." };

        var rateDto = _rateService.GetCurrentRate(currencyCode);
        if (rateDto.Error is not null)
            return new TransactionDto { Error = rateDto.Error };

        var costInPln = Math.Round(amount * rateDto.Mid, 2);
        var plnBalance = balances.GetOrAdd("PLN", 0m);

        if (plnBalance < costInPln)
            return new TransactionDto { Error = $"Insufficient PLN balance. Required: {costInPln}, Available: {plnBalance}." };

        balances.AddOrUpdate("PLN", 0m, (_, old) => old - costInPln);
        balances.AddOrUpdate(currencyCode.ToUpper(), amount, (_, old) => old + amount);

        var tx = new TransactionRecord
        {
            Id = DataStore.NextTransactionId(),
            UserId = userId,
            FromCurrency = "PLN",
            ToCurrency = currencyCode.ToUpper(),
            FromAmount = costInPln,
            ToAmount = amount,
            Rate = rateDto.Mid,
            Date = DateTime.UtcNow
        };
        DataStore.Transactions.Add(tx);

        return new TransactionDto
        {
            Id = tx.Id,
            FromCurrency = tx.FromCurrency,
            ToCurrency = tx.ToCurrency,
            FromAmount = tx.FromAmount,
            ToAmount = tx.ToAmount,
            Rate = tx.Rate,
            Date = tx.Date
        };
    }

    public TransactionDto SellCurrency(int userId, string currencyCode, decimal amount)
    {
        if (amount <= 0)
            return new TransactionDto { Error = "Amount must be greater than zero." };

        if (!DataStore.Balances.TryGetValue(userId, out var balances))
            return new TransactionDto { Error = "User not found." };

        var code = currencyCode.ToUpper();
        var foreignBalance = balances.GetOrAdd(code, 0m);

        if (foreignBalance < amount)
            return new TransactionDto { Error = $"Insufficient {code} balance. Required: {amount}, Available: {foreignBalance}." };

        var rateDto = _rateService.GetCurrentRate(currencyCode);
        if (rateDto.Error is not null)
            return new TransactionDto { Error = rateDto.Error };

        var gainInPln = Math.Round(amount * rateDto.Mid, 2);

        balances.AddOrUpdate(code, 0m, (_, old) => old - amount);
        balances.AddOrUpdate("PLN", gainInPln, (_, old) => old + gainInPln);

        var tx = new TransactionRecord
        {
            Id = DataStore.NextTransactionId(),
            UserId = userId,
            FromCurrency = code,
            ToCurrency = "PLN",
            FromAmount = amount,
            ToAmount = gainInPln,
            Rate = rateDto.Mid,
            Date = DateTime.UtcNow
        };
        DataStore.Transactions.Add(tx);

        return new TransactionDto
        {
            Id = tx.Id,
            FromCurrency = tx.FromCurrency,
            ToCurrency = tx.ToCurrency,
            FromAmount = tx.FromAmount,
            ToAmount = tx.ToAmount,
            Rate = tx.Rate,
            Date = tx.Date
        };
    }

    public List<TransactionDto> GetTransactionHistory(int userId) =>
        DataStore.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                FromCurrency = t.FromCurrency,
                ToCurrency = t.ToCurrency,
                FromAmount = t.FromAmount,
                ToAmount = t.ToAmount,
                Rate = t.Rate,
                Date = t.Date
            }).ToList();
}



