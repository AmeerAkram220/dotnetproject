using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using ExchangeOfficeService;

AppDb.EnsureCreated(); // initialise SQLite schema on first run

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServiceModelServices();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<ExchangeRateService>();
builder.Services.AddTransient<TransactionService>();

var app = builder.Build();

app.UseServiceModel(sb =>
{
    sb.AddService<AccountService>();
    sb.AddServiceEndpoint<AccountService, IAccountService>(
        new BasicHttpBinding(BasicHttpSecurityMode.None), "/AccountService.svc");

    sb.AddService<ExchangeRateService>();
    sb.AddServiceEndpoint<ExchangeRateService, IExchangeRateService>(
        new BasicHttpBinding(BasicHttpSecurityMode.None), "/ExchangeRateService.svc");

    sb.AddService<TransactionService>();
    sb.AddServiceEndpoint<TransactionService, ITransactionService>(
        new BasicHttpBinding(BasicHttpSecurityMode.None), "/TransactionService.svc");
});

app.Run();
