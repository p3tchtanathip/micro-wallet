using MediatR;

namespace Application.Common.Responses;

public record TransactionCompletedNotification(
    long TransactionId,
    string? Description,
    string Type,
    decimal Amount
) : INotification;