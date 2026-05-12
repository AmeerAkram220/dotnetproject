using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeOfficeService;

// ── In-memory stores (replaced by DB in Lab 11) ─────────────────────────────

public static class DataStore
{
    private static int _nextUserId = 1;
    private static int _nextTransactionId = 1;

    public static ConcurrentDictionary<int, UserRecord> Users { get; } = new();
    // userId -> (currencyCode -> amount)
    public static ConcurrentDictionary<int, ConcurrentDictionary<string, decimal>> Balances { get; } = new();
    public static ConcurrentBag<TransactionRecord> Transactions { get; } = new();

    public static int NextUserId() => Interlocked.Increment(ref _nextUserId);
    public static int NextTransactionId() => Interlocked.Increment(ref _nextTransactionId);
}

public class UserRecord
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class TransactionRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }
    public decimal Rate { get; set; }
    public DateTime Date { get; set; }
}

public static class PasswordHelper
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string hash) =>
        Hash(password) == hash;
}
