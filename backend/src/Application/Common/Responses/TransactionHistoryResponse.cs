namespace Application.Common.Responses;

public record TransactionHistoryResponse(
    string ReferenceNo,
    string Type,
    string Status,
    decimal Amount,
    string? Description,
    DateTime CreatedAt,
    string? CounterpartyName,
    long? CounterpartyWalletId,
    string Direction
);