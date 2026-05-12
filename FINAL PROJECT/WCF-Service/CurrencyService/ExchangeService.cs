using System.Net.Http.Json;

namespace CurrencyService;

public class ExchangeService : ICurrencyService
{
    private static readonly HttpClient _httpClient = new();

    public ExchangeRateResult GetExchangeRate(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return new ExchangeRateResult { Error = "Currency code cannot be empty." };

        try
        {
            var url = $"https://api.nbp.pl/api/exchangerates/rates/a/{currencyCode.ToLower()}/?format=json";
            var response = _httpClient.GetFromJsonAsync<NbpResponse>(url).GetAwaiter().GetResult();

            if (response?.Rates == null || response.Rates.Length == 0)
                return new ExchangeRateResult { Error = $"No data found for: {currencyCode.ToUpper()}" };

            return new ExchangeRateResult
            {
                Currency = response.Currency,
                Code = response.Code,
                Mid = response.Rates[0].Mid
            };
        }
        catch (HttpRequestException)
        {
            return new ExchangeRateResult { Error = $"Currency '{currencyCode.ToUpper()}' not found or NBP API unavailable." };
        }
    }
}
