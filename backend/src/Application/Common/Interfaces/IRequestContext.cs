using Domain.Constants;

namespace Application.Common.Interfaces;

public interface IRequestContext
{
    string? UserId { get; }
    string? Role { get; }
    string? IdempotencyKey { get; }

    bool IsAdmin => Role == RoleNames.Admin;
}
