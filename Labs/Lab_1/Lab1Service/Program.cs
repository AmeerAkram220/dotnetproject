var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/message", () => "Hello from the modern Web API, I couldn't use WCF because I'm on macOS!");

app.Run();