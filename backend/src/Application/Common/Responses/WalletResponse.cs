namespace Application.Common.Responses;

public record class WalletResponse(
    long WalletId,
    string WalletNumber,
    decimal Balance,
    string Currency
);