using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Wallets.Commands.Deposit;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTests.Fixtures;

namespace UnitTests.Application.Wallets.Commands.Deposit;

public class DepositCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly Mock<IPaymentGatewayService> _paymentGatewayServiceMock;
    private readonly DepositCommandHandler _handler;

    public DepositCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _requestContextMock = new Mock<IRequestContext>();
        _paymentGatewayServiceMock = new Mock<IPaymentGatewayService>();

        _handler = new DepositCommandHandler(_context, _paymentGatewayServiceMock.Object, _requestContextMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<Wallet> SeedWalletAsync(long userId = 1, decimal balance = 0)
    {
        var user = new User
        {
            Id = userId,
            Email = $"user{userId}@example.com",
            Password = "hashed_password",
            Provider = AuthProvider.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var wallet = new Wallet
        {
            UserId = userId,
            WalletNumber = "1234567890",
            Balance = balance,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }

    // Unauthorized
    [Fact]
    public async Task Handle_WithoutUserId_ShouldThrowUnauthorizedAccessException()
    {
        _requestContextMock.SetupGet(r => r.UserId).Returns((string?)null);

        var command = new DepositCommand(1, 100);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }

    // Not Found
    [Fact]
    public async Task Handle_WhenWalletNotFound_ShouldThrowNotFoundException()
    {
        _requestContextMock.SetupGet(r => r.UserId).Returns("1");

        var command = new DepositCommand(999, 100);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("Wallet not found", exception.Message);
    }

    // Forbidden
    [Fact]
    public async Task Handle_WhenWalletBelongsToAnotherUser_ShouldThrowForbiddenAccessException()
    {
        var wallet = await SeedWalletAsync(userId: 1);

        _requestContextMock.SetupGet(r => r.UserId).Returns("2");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);

        var command = new DepositCommand(wallet.Id, 100);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }

    // Admin bypass ownership check
    [Fact]
    public async Task Handle_WhenAdminDepositToAnotherUserWallet_ShouldSucceed()
    {
        var wallet = await SeedWalletAsync(userId: 1);

        _requestContextMock.SetupGet(r => r.UserId).Returns("99");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(true);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(100))
            .ReturnsAsync(new GatewayResult(true, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Success"));

        var command = new DepositCommand(wallet.Id, 100);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Success", result.Status);
    }

    // Happy Path
    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        var wallet = await SeedWalletAsync(userId: 1, balance: 500);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(100))
            .ReturnsAsync(new GatewayResult(true, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Success"));

        var command = new DepositCommand(wallet.Id, 100);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Success", result.Status);
        Assert.Equal(100, result.Amount);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldIncreaseWalletBalance()
    {
        var wallet = await SeedWalletAsync(userId: 1, balance: 500);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(100))
            .ReturnsAsync(new GatewayResult(true, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Success"));

        var command = new DepositCommand(wallet.Id, 100);
        await _handler.Handle(command, CancellationToken.None);

        var updatedWallet = await _context.Wallets.FindAsync(wallet.Id);
        Assert.Equal(600, updatedWallet!.Balance);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldSaveTransactionToDatabase()
    {
        var wallet = await SeedWalletAsync(userId: 1);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(200))
            .ReturnsAsync(new GatewayResult(true, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Success"));

        var command = new DepositCommand(wallet.Id, 200);
        await _handler.Handle(command, CancellationToken.None);

        var transaction = await _context.Transactions.FirstOrDefaultAsync();
        Assert.NotNull(transaction);
        Assert.Equal(TransactionType.Deposit, transaction.Type);
        Assert.Equal(TransactionStatus.Success, transaction.Status);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldSaveTransactionEntryToDatabase()
    {
        var wallet = await SeedWalletAsync(userId: 1);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(200))
            .ReturnsAsync(new GatewayResult(true, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Success"));

        var command = new DepositCommand(wallet.Id, 200);
        await _handler.Handle(command, CancellationToken.None);

        var entry = await _context.TransactionEntries.FirstOrDefaultAsync();
        Assert.NotNull(entry);
        Assert.Equal(200, entry.Amount);
    }

    // Gateway failed
    [Fact]
    public async Task Handle_WhenGatewayFails_ShouldReturnFailedStatus()
    {
        var wallet = await SeedWalletAsync(userId: 1, balance: 500);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(100))
            .ReturnsAsync(new GatewayResult(false, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Failed"));

        var command = new DepositCommand(wallet.Id, 100);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("Failed", result.Status);
    }

    [Fact]
    public async Task Handle_WhenGatewayFails_ShouldNotChangeWalletBalance()
    {
        var wallet = await SeedWalletAsync(userId: 1, balance: 500);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns((string?)null);
        _paymentGatewayServiceMock
            .Setup(g => g.DepositAsync(100))
            .ReturnsAsync(new GatewayResult(false, "7438bfb3-72c6-4bd2-9cee-f2ef68b7a94c", "Failed"));

        var command = new DepositCommand(wallet.Id, 100);
        await _handler.Handle(command, CancellationToken.None);

        var updatedWallet = await _context.Wallets.FindAsync(wallet.Id);
        Assert.Equal(500, updatedWallet!.Balance);
    }

    // Idempotency
    [Fact]
    public async Task Handle_WithExistingIdempotencyKey_ShouldReturnExistingTransaction()
    {
        var wallet = await SeedWalletAsync(userId: 1, balance: 500);
        var idempotencyKey = "unique-key-123";

        var existingTransaction = new Transaction
        {
            ReferenceNo = "existing-ref",
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Success,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            TransactionEntries = [new TransactionEntry { WalletId = wallet.Id, Amount = 100 }]
        };

        _context.Transactions.Add(existingTransaction);
        await _context.SaveChangesAsync();

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _requestContextMock.SetupGet(r => r.IsAdmin).Returns(false);
        _requestContextMock.SetupGet(r => r.IdempotencyKey).Returns(idempotencyKey);

        var command = new DepositCommand(wallet.Id, 100);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("existing-ref", result.ReferenceNo);
        _paymentGatewayServiceMock.Verify(g => g.DepositAsync(It.IsAny<decimal>()), Times.Never);
    }
}
