using System.Net.Http.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private const string CacheKey = "usd-thb-rate";

    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;

    public ExchangeRateService(HttpClient httpClient, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<decimal> GetUsdToThbRateAsync(CancellationToken ct)
    {
        var cached = await _cache.GetStringAsync(CacheKey, ct);

        if (!string.IsNullOrWhiteSpace(cached))
        {
            return decimal.Parse(cached);
        }

        var response = await _httpClient.GetFromJsonAsync<ExchangeRateResponse>("latest?from=USD&to=THB", ct);

        if (response == null || response.Rates == null)
        {
            throw new BadGatewayException("Exchange provider unavailable");
        }

        if (!response.Rates.TryGetValue("THB", out var rate))
        {
            throw new BadGatewayException("THB exchange rate not found");
        }

        if (rate <= 0 || rate > 1000)
        {
            throw new BadGatewayException("Invalid exchange rate received");
        }

        await _cache.SetStringAsync(
            CacheKey,
            rate.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            ct);

        return rate;
    }
}
