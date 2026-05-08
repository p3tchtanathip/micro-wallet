namespace Application.Common.Responses;

public record class TransactionResponse(
    string ReferenceNo,
    string Status,
    decimal Amount,
    decimal BalanceAfter,
    DateTime CreatedAt
);
