using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", () => "The Currency Exchange API is running! Try going to /exchange/usd");
app.MapGet("/exchange/{code}", async (string code, HttpClient client) =>
{
    string nbpUrl = $"http://api.nbp.pl/api/exchangerates/rates/a/{code}/?format=json";

    try
    {
        var nbpData = await client.GetFromJsonAsync<NbpResponse>(nbpUrl);
        if (nbpData != null && nbpData.Rates.Length > 0)
        {
            return Results.Ok(new 
            {
                CurrencyName = nbpData.Currency,
                CurrencyCode = nbpData.Code,
                ExchangeRate = nbpData.Rates[0].Mid
            });
        }
        
        return Results.NotFound("Data not found for this currency.");
    }
    catch (HttpRequestException)
    {
        return Results.BadRequest("Invalid currency code or the NBP API is down.");
    }
});

app.Run();
class NbpResponse
{
    public string Currency { get; set; }
    public string Code { get; set; }
    public NbpRate[] Rates { get; set; }
}

class NbpRate
{
    public decimal Mid { get; set; } 
}