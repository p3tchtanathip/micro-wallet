using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Context;

public class RequestContext : IRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?
            .User
            ?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Role =>
        _httpContextAccessor.HttpContext?
            .User
            ?.FindFirstValue(ClaimTypes.Role);

    public string? IdempotencyKey =>
        _httpContextAccessor.HttpContext?
            .Request.Headers["Idempotency-Key"]
            .FirstOrDefault();
}