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
    Date         TEXT    NOT NULL,   -- ISO-8601: 'YYYY-MM-DD HH:MM:SS'
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
