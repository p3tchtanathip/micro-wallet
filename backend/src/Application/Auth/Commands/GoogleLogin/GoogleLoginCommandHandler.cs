using Application.Auth.Commands.Login;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;    
    private readonly IGoogleAuthService _googleAuthService;

    public GoogleLoginCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher,  IJwtTokenGenerator jwtTokenGenerator, IGoogleAuthService googleAuthService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _googleAuthService = googleAuthService;
    }

    public async Task<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var payload = await _googleAuthService.VerifyTokenAsync(request.IdToken);
        
        if (payload == null)
            throw new UnauthorizedAccessException("Invalid Google Token");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email, ct);

        if (user == null)
        {
            user = new User {
                Email = payload.Email,
                FullName = payload.Name,
                Password = _passwordHasher.Hash(Guid.NewGuid().ToString()),
                Provider = AuthProvider.Google
            };
            _context.Users.Add(user);
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, refreshToken);
    }   
}
