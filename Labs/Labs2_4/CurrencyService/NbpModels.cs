using System.Text.Json.Serialization;

namespace CurrencyService;

internal record NbpResponse(
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("rates")] NbpRate[] Rates
);

internal record NbpRate([property: JsonPropertyName("mid")] decimal Mid);
