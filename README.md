# Currency Exchange Office System

**Course:** Network Application Development
**Author:** Amir Morkos
**Student ID:** 64524

## Description

A network-based system simulating an online currency exchange office, built using Windows Communication Foundation (WCF) and .NET. The system exposes WCF services for retrieving real exchange rates from the National Bank of Poland (NBP) public API and will grow into a full exchange office platform with user accounts, transactions, and a WPF client.

## Project Structure

```
dotnetproject/
├── Labs/
│   ├── Lab_1/
│   │   ├── Lab1Service/        WCF service exposing a simple greeting operation
│   │   └── Lab1Client/         Console client that consumes Lab1Service via WCF
│   └── Labs2_4/
│       └── CurrencyService/    WCF service returning live exchange rates from NBP API
└── README.md
```

## How to Run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

### Lab 1 — WCF Hello World

**1. Start the service**

```bash
cd Labs/Lab_1/Lab1Service
dotnet run
```

The WCF endpoint is available at: `http://localhost:5124/SimpleService.svc`

**2. Run the client** (in a separate terminal)

```bash
cd Labs/Lab_1/Lab1Client
dotnet run
```

The client connects to the service and prints the response.

---

### Labs 2–4 — Currency Exchange Rate Service

**Start the service**

```bash
cd Labs/Labs2_4/CurrencyService
dotnet run
```

The WCF endpoint is available at: `http://localhost:5022/CurrencyService.svc`

To test, consume the service via a WCF client or inspect the WSDL at:
`http://localhost:5022/CurrencyService.svc?wsdl`

The `GetExchangeRate` operation accepts a currency code (e.g. `USD`, `EUR`, `GBP`) and returns the current mid-rate from the NBP API.

---

## Progress

- [x] Lab 1 — WCF Hello World Service + Console Client
- [x] Labs 2–4 — Currency Exchange Rate WCF Service (NBP API)
- [ ] Labs 5–14 — Full Currency Exchange Office System (WCF + WPF + Database)
