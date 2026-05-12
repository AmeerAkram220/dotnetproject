using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ExchangeOfficeService;

public static class NbpClient
{
    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private const string BaseUrl = "https://api.nbp.pl/api";

    /// <summary>Returns the current mid-rate for a single currency from table A.</summary>
    public static async Task<NbpSingleResponse?> GetCurrentRateAsync(string currencyCode)
    {
        var url = $"{BaseUrl}/exchangerates/rates/a/{currencyCode.ToLower()}/?format=json";
        return await _http.GetFromJsonAsync<NbpSingleResponse>(url);
    }

    /// <summary>Returns mid-rates for a single currency over a date range (max 367 days).</summary>
    public static async Task<NbpSingleResponse?> GetHistoricalRatesAsync(string currencyCode, DateTime from, DateTime to)
    {
        var fromStr = from.ToString("yyyy-MM-dd");
        var toStr = to.ToString("yyyy-MM-dd");
        var url = $"{BaseUrl}/exchangerates/rates/a/{currencyCode.ToLower()}/{fromStr}/{toStr}/?format=json";
        return await _http.GetFromJsonAsync<NbpSingleResponse>(url);
    }

    /// <summary>Returns the full current table A (all currencies).</summary>
    public static async Task<NbpTableResponse[]?> GetAllCurrentRatesAsync()
    {
        var url = $"{BaseUrl}/exchangerates/tables/a/?format=json";
        return await _http.GetFromJsonAsync<NbpTableResponse[]>(url);
    }
}

// ── NBP API response models ──────────────────────────────────────────────────

public record NbpSingleResponse(
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("rates")] NbpRateEntry[] Rates
);

public record NbpRateEntry(
    [property: JsonPropertyName("mid")] decimal Mid,
    [property: JsonPropertyName("effectiveDate")] DateTime EffectiveDate
);

public record NbpTableResponse(
    [property: JsonPropertyName("table")] string Table,
    [property: JsonPropertyName("no")] string No,
    [property: JsonPropertyName("effectiveDate")] DateTime EffectiveDate,
    [property: JsonPropertyName("rates")] NbpTableRate[] Rates
);

public record NbpTableRate(
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("mid")] decimal Mid
);
