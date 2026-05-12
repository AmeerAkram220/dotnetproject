using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using Lab1Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServiceModelServices();
builder.Services.AddTransient<SimpleService>();

var app = builder.Build();

app.UseServiceModel(sb =>
{
    sb.AddService<SimpleService>();
    sb.AddServiceEndpoint<SimpleService, ISimpleService>(
        new BasicHttpBinding(BasicHttpSecurityMode.None), "/SimpleService.svc");
});

app.Run();