using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CurrencyService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServiceModelServices();
builder.Services.AddTransient<ExchangeService>();

var app = builder.Build();

app.UseServiceModel(sb =>
{
    sb.AddService<ExchangeService>();
    sb.AddServiceEndpoint<ExchangeService, ICurrencyService>(
        new BasicHttpBinding(BasicHttpSecurityMode.None), "/CurrencyService.svc");
});

app.Run();
