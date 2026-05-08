namespace Application.Common.Responses;

public record BalanceSummaryResponse(
    string WalletNumber,
    decimal CurrentBalance,
    string Currency,
    DateTime? LastUpdated
);