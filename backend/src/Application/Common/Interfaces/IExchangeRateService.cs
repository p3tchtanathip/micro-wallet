namespace Application.Common.Interfaces;

public interface IExchangeRateService
{
    Task<decimal> GetUsdToThbRateAsync(CancellationToken ct);
}