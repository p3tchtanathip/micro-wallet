using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, long>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<long> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already exists");

        // Hash Password
        var user = new User {
            Email = request.Email,
            Password = _passwordHasher.Hash(request.Password),
            FullName = request.FullName
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return user.Id;
    }
}
