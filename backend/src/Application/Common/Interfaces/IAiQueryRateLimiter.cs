namespace Application.Common.Interfaces;

public interface IAiQueryRateLimiter
{
    Task<bool> CanUseAiAsync(string userId);
}
