namespace Application.Common.Responses;

public record WalletResponse(
    long WalletId,
    string WalletNumber,
    decimal Balance,
    string Currency
);