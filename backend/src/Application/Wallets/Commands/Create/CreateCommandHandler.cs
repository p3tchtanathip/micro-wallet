using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Commands.Create;

public class CreateCommandHandler : IRequestHandler<CreateCommand, WalletResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;
    private readonly IWalletService _walletService;

    public CreateCommandHandler(IApplicationDbContext context, IRequestContext requestContext, IWalletService walletService)
    {
        _context = context;
        _requestContext = requestContext;
        _walletService = walletService;
    }

    public async Task<WalletResponse> Handle(CreateCommand request, CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();
        var currentUserId = long.Parse(_requestContext.UserId);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId, ct)
            ?? throw new NotFoundException("User not found");

        var walletNumber = await _walletService.GenerateUniqueWalletNumberAsync();

        var wallet = new Wallet
        {
            User = user,
            WalletNumber = walletNumber,
            Currency = request.Currency,
            Balance = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync(ct);

        return new WalletResponse(
            wallet.Id,
            wallet.WalletNumber,
            wallet.Balance,
            wallet.Currency);
    }
}
