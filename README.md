# Currency Exchange Office System

**Course:** Network Application Development
**Author:** Amir Morkos
**Student ID:** 64524

## Description

A network-based system simulating an online currency exchange office, built using Windows Communication Foundation (WCF) and .NET. The system exposes WCF services for retrieving real exchange rates from the National Bank of Poland (NBP) public API, and is being extended into a full exchange office platform with user accounts, currency transactions, and a WPF client application backed by a database.

## Project Structure

```
dotnetproject/
└── FINAL PROJECT/
    ├── CurrencyExchangeOffice.sln
    ├── WCF-Service/
    │   ├── Lab1Service/              Lab 1 WCF service (GetMessage)
    │   ├── Lab1Client/               Lab 1 console client
    │   ├── CurrencyService/          Labs 2–4 currency rate WCF service (NBP API)
    │   └── ExchangeOfficeService/    Final project WCF service (3 endpoints)
    ├── Client-Application/           WPF client (in progress)
    ├── Database/                     SQL schema & scripts (in progress)
    └── Documentation/
```

## How to Run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

### Lab 1 — WCF Hello World

**1. Start the service**

```bash
cd "FINAL PROJECT/WCF-Service/Lab1Service"
dotnet run
```

WCF endpoint: `http://localhost:5000/SimpleService.svc`

**2. Run the client** (separate terminal)

```bash
cd "FINAL PROJECT/WCF-Service/Lab1Client"
dotnet run
```

---

### Labs 2–4 — Currency Exchange Rate Service

```bash
cd "FINAL PROJECT/WCF-Service/CurrencyService"
dotnet run
```

WCF endpoint: `http://localhost:5000/CurrencyService.svc`

`GetExchangeRate(currencyCode)` — accepts a currency code (e.g. `USD`, `EUR`, `GBP`) and returns the current mid-rate from the NBP API.

---

### Labs 5+ — Final Project (Exchange Office Service)

```bash
cd "FINAL PROJECT/WCF-Service/ExchangeOfficeService"
dotnet run
```

Exposes three WCF endpoints:
- `/AccountService.svc` — user registration, login, balance top-up
- `/ExchangeRateService.svc` — current & historical exchange rates
- `/TransactionService.svc` — buy/sell currency, transaction history

Test via Postman (SOAP) or SoapUI.

---

## Progress

- [x] Lab 1 — WCF Hello World Service + Console Client
- [x] Labs 2–4 — Currency Exchange Rate WCF Service (NBP API)
- [x] Lab 5 — Final project architecture: 3 WCF service contracts defined (Account, ExchangeRate, Transaction)
- [x] Lab 6 — Currency exchange business logic (in-memory): register/login, top-up, buy/sell currency, transaction history
- [x] Lab 7 — NBP API integration: dedicated `NbpClient`, `GetCurrentRate`, `GetHistoricalRates`, `GetAllCurrentRates`
- [ ] Lab 8 — WPF client application skeleton
- [ ] Lab 9 — User account management
- [ ] Lab 10 — Buy/sell currency transactions in WPF
- [ ] Lab 11 — SQL database schema & scripts
- [ ] Lab 12 — Persist transactions & balances in DB
- [ ] Lab 13 — Historical rates view & reporting
- [ ] Lab 14 — Testing, debugging, final fixes
