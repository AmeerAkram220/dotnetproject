using Microsoft.Data.Sqlite;

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

        using var conn = AppDb.Open();

        // Check duplicate
        using (var chk = conn.CreateCommand())
        {
            chk.CommandText = "SELECT COUNT(*) FROM Users WHERE LOWER(Username) = LOWER($u)";
            chk.Parameters.AddWithValue("$u", username.Trim());
            if ((long)chk.ExecuteScalar()! > 0)
                return new UserDto { Error = "Username already taken." };
        }

        // Insert user
        long newId;
        using (var ins = conn.CreateCommand())
        {
            ins.CommandText = """
                INSERT INTO Users (Username, PasswordHash)
                VALUES ($u, $h);
                SELECT last_insert_rowid();
                """;
            ins.Parameters.AddWithValue("$u", username.Trim());
            ins.Parameters.AddWithValue("$h", PasswordHelper.Hash(password));
            newId = (long)ins.ExecuteScalar()!;
        }

        // Seed PLN balance = 0
        using (var bal = conn.CreateCommand())
        {
            bal.CommandText = """
                INSERT INTO Balances (UserId, CurrencyCode, Amount)
                VALUES ($id, 'PLN', 0)
                ON CONFLICT(UserId, CurrencyCode) DO NOTHING;
                """;
            bal.Parameters.AddWithValue("$id", newId);
            bal.ExecuteNonQuery();
        }

        return new UserDto { Id = (int)newId, Username = username.Trim() };
    }

    public UserDto Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new UserDto { Error = "Username and password are required." };

        using var conn = AppDb.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, PasswordHash FROM Users WHERE LOWER(Username) = LOWER($u)";
        cmd.Parameters.AddWithValue("$u", username.Trim());

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return new UserDto { Error = "Invalid username or password." };

        var id = reader.GetInt32(0);
        var storedName = reader.GetString(1);
        var hash = reader.GetString(2);

        if (!PasswordHelper.Verify(password, hash))
            return new UserDto { Error = "Invalid username or password." };

        return new UserDto { Id = id, Username = storedName };
    }

    public OperationResult TopUpBalance(int userId, decimal amount)
    {
        if (amount <= 0)
            return new OperationResult { Success = false, Error = "Amount must be greater than zero." };

        using var conn = AppDb.Open();

        // Verify user exists
        using (var chk = conn.CreateCommand())
        {
            chk.CommandText = "SELECT COUNT(*) FROM Users WHERE Id = $id";
            chk.Parameters.AddWithValue("$id", userId);
            if ((long)chk.ExecuteScalar()! == 0)
                return new OperationResult { Success = false, Error = "User not found." };
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Balances (UserId, CurrencyCode, Amount)
            VALUES ($id, 'PLN', $amt)
            ON CONFLICT(UserId, CurrencyCode)
            DO UPDATE SET Amount = Amount + $amt;
            """;
        cmd.Parameters.AddWithValue("$id", userId);
        cmd.Parameters.AddWithValue("$amt", (double)amount);
        cmd.ExecuteNonQuery();

        return new OperationResult { Success = true };
    }

    public List<BalanceDto> GetBalances(int userId)
    {
        using var conn = AppDb.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT CurrencyCode, Amount FROM Balances
            WHERE UserId = $id AND Amount > 0
            ORDER BY CurrencyCode
            """;
        cmd.Parameters.AddWithValue("$id", userId);

        var result = new List<BalanceDto>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new BalanceDto
            {
                CurrencyCode = reader.GetString(0),
                Amount = (decimal)reader.GetDouble(1)
            });
        return result;
    }

    public OperationResult ChangePassword(int userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return new OperationResult { Success = false, Error = "New password must be at least 6 characters." };

        using var conn = AppDb.Open();
        using var sel = conn.CreateCommand();
        sel.CommandText = "SELECT PasswordHash FROM Users WHERE Id = $id";
        sel.Parameters.AddWithValue("$id", userId);
        var existing = sel.ExecuteScalar() as string;

        if (existing is null)
            return new OperationResult { Success = false, Error = "User not found." };

        if (!PasswordHelper.Verify(currentPassword, existing))
            return new OperationResult { Success = false, Error = "Current password is incorrect." };

        using var upd = conn.CreateCommand();
        upd.CommandText = "UPDATE Users SET PasswordHash = $h WHERE Id = $id";
        upd.Parameters.AddWithValue("$h", PasswordHelper.Hash(newPassword));
        upd.Parameters.AddWithValue("$id", userId);
        upd.ExecuteNonQuery();

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

        var code = currencyCode.ToUpper();

        var rateDto = _rateService.GetCurrentRate(code);
        if (rateDto.Error is not null)
            return new TransactionDto { Error = rateDto.Error };

        var costInPln = Math.Round(amount * rateDto.Mid, 2);

        using var conn = AppDb.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            // Check PLN balance
            decimal plnBalance = GetBalance(conn, userId, "PLN");
            if (plnBalance < costInPln)
            {
                tx.Rollback();
                return new TransactionDto { Error = $"Insufficient PLN balance. Required: {costInPln:F2}, Available: {plnBalance:F2}." };
            }

            // Deduct PLN
            UpsertBalance(conn, userId, "PLN", plnBalance - costInPln);
            // Add foreign currency
            decimal foreignBalance = GetBalance(conn, userId, code);
            UpsertBalance(conn, userId, code, foreignBalance + amount);

            // Record transaction
            long txId = InsertTransaction(conn, userId, "PLN", code, costInPln, amount, rateDto.Mid);

            tx.Commit();

            return new TransactionDto
            {
                Id = (int)txId,
                FromCurrency = "PLN",
                ToCurrency = code,
                FromAmount = costInPln,
                ToAmount = amount,
                Rate = rateDto.Mid,
                Date = DateTime.UtcNow
            };
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public TransactionDto SellCurrency(int userId, string currencyCode, decimal amount)
    {
        if (amount <= 0)
            return new TransactionDto { Error = "Amount must be greater than zero." };

        var code = currencyCode.ToUpper();

        var rateDto = _rateService.GetCurrentRate(code);
        if (rateDto.Error is not null)
            return new TransactionDto { Error = rateDto.Error };

        using var conn = AppDb.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            decimal foreignBalance = GetBalance(conn, userId, code);
            if (foreignBalance < amount)
            {
                tx.Rollback();
                return new TransactionDto { Error = $"Insufficient {code} balance. Required: {amount:F4}, Available: {foreignBalance:F4}." };
            }


            var gainInPln = Math.Round(amount * rateDto.Mid, 2);

            // Deduct foreign
            UpsertBalance(conn, userId, code, foreignBalance - amount);
            // Add PLN
            decimal plnBalance = GetBalance(conn, userId, "PLN");
            UpsertBalance(conn, userId, "PLN", plnBalance + gainInPln);

            long txId = InsertTransaction(conn, userId, code, "PLN", amount, gainInPln, rateDto.Mid);

            tx.Commit();

            return new TransactionDto
            {
                Id = (int)txId,
                FromCurrency = code,
                ToCurrency = "PLN",
                FromAmount = amount,
                ToAmount = gainInPln,
                Rate = rateDto.Mid,
                Date = DateTime.UtcNow
            };
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public List<TransactionDto> GetTransactionHistory(int userId)
    {
        using var conn = AppDb.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT Id, FromCurrency, ToCurrency, FromAmount, ToAmount, Rate, Date
            FROM Transactions
            WHERE UserId = $id
            ORDER BY Date DESC
            """;
        cmd.Parameters.AddWithValue("$id", userId);

        var result = new List<TransactionDto>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new TransactionDto
            {
                Id = reader.GetInt32(0),
                FromCurrency = reader.GetString(1),
                ToCurrency = reader.GetString(2),
                FromAmount = (decimal)reader.GetDouble(3),
                ToAmount = (decimal)reader.GetDouble(4),
                Rate = (decimal)reader.GetDouble(5),
                Date = DateTime.Parse(reader.GetString(6))
            });
        return result;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static decimal GetBalance(SqliteConnection conn, int userId, string code)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Amount FROM Balances WHERE UserId = $id AND CurrencyCode = $c";
        cmd.Parameters.AddWithValue("$id", userId);
        cmd.Parameters.AddWithValue("$c", code);
        var val = cmd.ExecuteScalar();
        return val is null ? 0m : (decimal)(double)val;
    }

    private static void UpsertBalance(SqliteConnection conn, int userId, string code, decimal amount)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Balances (UserId, CurrencyCode, Amount)
            VALUES ($id, $c, $amt)
            ON CONFLICT(UserId, CurrencyCode)
            DO UPDATE SET Amount = $amt;
            """;
        cmd.Parameters.AddWithValue("$id", userId);
        cmd.Parameters.AddWithValue("$c", code);
        cmd.Parameters.AddWithValue("$amt", (double)amount);
        cmd.ExecuteNonQuery();
    }

    private static long InsertTransaction(SqliteConnection conn, int userId,
        string from, string to, decimal fromAmt, decimal toAmt, decimal rate)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Transactions (UserId, FromCurrency, ToCurrency, FromAmount, ToAmount, Rate, Date)
            VALUES ($uid, $fc, $tc, $fa, $ta, $r, $d);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$uid", userId);
        cmd.Parameters.AddWithValue("$fc", from);
        cmd.Parameters.AddWithValue("$tc", to);
        cmd.Parameters.AddWithValue("$fa", (double)fromAmt);
        cmd.Parameters.AddWithValue("$ta", (double)toAmt);
        cmd.Parameters.AddWithValue("$r", (double)rate);
        cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return (long)cmd.ExecuteScalar()!;
    }
}
