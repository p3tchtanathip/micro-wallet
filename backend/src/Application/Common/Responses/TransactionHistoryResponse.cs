namespace Application.Common.Responses;

public record TransactionHistoryResponse(
    string ReferenceNo,
    string Type,
    string Status,
    decimal Amount,
    string? Description,
    string? Category,
    DateTime CreatedAt,
    string? CounterpartyName,
    long? CounterpartyWalletId,
    string Direction
);