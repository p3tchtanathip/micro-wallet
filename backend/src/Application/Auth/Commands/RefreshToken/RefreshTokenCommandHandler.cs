using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, ct);

        if (user == null || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); 
        
        await _context.SaveChangesAsync(ct);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken);
    }
}
