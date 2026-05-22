using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services;

public class AiQueryRateLimiter : IAiQueryRateLimiter
{
    private const int DailyLimit = 20;
    private readonly IDistributedCache _cache;

    public AiQueryRateLimiter(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> CanUseAiAsync(string userId)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var key = $"ai_limit:{userId}:{today}";

        var count = await _cache.GetStringAsync(key);

        if (count == null)
        {
            await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2)
            });
            return true;
        }

        if (int.TryParse(count, out var current) && current >= DailyLimit)
            return false;

        await _cache.SetStringAsync(key, (current + 1).ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2)
        });
        return true;
    }
}
