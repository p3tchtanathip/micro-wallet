using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, long>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IWalletService _walletService;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IWalletService walletService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _walletService = walletService;
    }

    public async Task<long> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already exists");

        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == RoleNames.User, ct)
            ?? throw new Exception("Default role not found");

        // Hash Password
        var user = new User {
            Email = request.Email,
            Password = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            UserRoles = new List<UserRole>
            {
                new() { Role = defaultRole }
            }
        };

        _context.Users.Add(user);

        await _walletService.CreateDefaultWalletAsync(user);
        await _context.SaveChangesAsync(ct);

        return user.Id;
    }
}
