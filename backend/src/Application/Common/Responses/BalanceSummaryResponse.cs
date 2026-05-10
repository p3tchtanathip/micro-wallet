namespace Application.Common.Responses;

public record BalanceSummaryResponse(
    decimal TotalBalanceTHB,
    decimal ExchangeRate,
    DateTime? LastUpdated
);