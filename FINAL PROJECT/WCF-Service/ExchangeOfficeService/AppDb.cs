using Microsoft.Data.Sqlite;

namespace ExchangeOfficeService;

/// <summary>
/// Manages the SQLite connection and ensures the schema exists on startup.
/// The .db file is created next to the running executable.
/// </summary>
public static class AppDb
{
    private static readonly string _dbPath =
        Path.Combine(AppContext.BaseDirectory, "exchange_office.db");

    public static string ConnectionString { get; } =
        new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();

    /// <summary>
    /// Creates tables if they do not already exist.
    /// Call once from Program.cs before the service starts.
    /// </summary>
    public static void EnsureCreated()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                Username     TEXT    NOT NULL UNIQUE,
                PasswordHash TEXT    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Balances (
                UserId       INTEGER NOT NULL,
                CurrencyCode TEXT    NOT NULL,
                Amount       REAL    NOT NULL DEFAULT 0,
                PRIMARY KEY (UserId, CurrencyCode),
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );

            CREATE TABLE IF NOT EXISTS Transactions (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId       INTEGER NOT NULL,
                FromCurrency TEXT    NOT NULL,
                ToCurrency   TEXT    NOT NULL,
                FromAmount   REAL    NOT NULL,
                ToAmount     REAL    NOT NULL,
                Rate         REAL    NOT NULL,
                Date         TEXT    NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
            """;
        cmd.ExecuteNonQuery();
    }

    /// <summary>Opens and returns a new, already-open SQLite connection.</summary>
    public static SqliteConnection Open()
    {
        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }
}
