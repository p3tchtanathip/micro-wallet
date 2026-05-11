using Microsoft.EntityFrameworkCore;
using Application.Wallets.Commands.Create;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Moq;
using UnitTests.Fixtures;

namespace UnitTests.Application.Wallets.Commands.Create;

public class CreateCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IRequestContext> _requestContextMock;
    private readonly Mock<IWalletService> _walletServiceMock;
    private readonly CreateCommandHandler _handler;

    public CreateCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _requestContextMock = new Mock<IRequestContext>();
        _walletServiceMock = new Mock<IWalletService>();

        _handler = new CreateCommandHandler(_context, _requestContextMock.Object, _walletServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task SeedUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateWallet()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Password = "hashed_password",
            Provider = AuthProvider.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await SeedUserAsync(user);

        _requestContextMock.SetupGet(r => r.UserId).Returns("1");
        _walletServiceMock.Setup(w => w.GenerateUniqueWalletNumberAsync()).ReturnsAsync("1234567890");

        var command = new CreateCommand(Currencies.THB);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("1234567890", result.WalletNumber);
        Assert.Equal(Currencies.THB, result.Currency);
        Assert.Equal(0, result.Balance);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallGenerateUniqueWalletNumber()
    {
        var user = new User
        {
            Id = 2,
            Email = "test2@example.com",
            Password = "hashed_password",
            Provider = AuthProvider.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await SeedUserAsync(user);

        _requestContextMock.SetupGet(r => r.UserId).Returns("2");
        _walletServiceMock.Setup(w => w.GenerateUniqueWalletNumberAsync()).ReturnsAsync("1234567890");

        var command = new CreateCommand(Currencies.USD);
        await _handler.Handle(command, CancellationToken.None);

        _walletServiceMock.Verify(w => w.GenerateUniqueWalletNumberAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldSaveWalletToDatabase()
    {
        var user = new User
        {
            Id = 3,
            Email = "test3@example.com",
            Password = "hashed_password",
            Provider = AuthProvider.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await SeedUserAsync(user);

        _requestContextMock.SetupGet(r => r.UserId).Returns("3");
        _walletServiceMock.Setup(w => w.GenerateUniqueWalletNumberAsync()).ReturnsAsync("1234567890");

        var command = new CreateCommand(Currencies.THB);
        await _handler.Handle(command, CancellationToken.None);

        var walletCount = await _context.Wallets.CountAsync();
        Assert.Equal(1, walletCount);
    }

    [Fact]
    public async Task Handle_WithoutUserId_ShouldThrowUnauthorizedAccessException()
    {
        _requestContextMock.SetupGet(r => r.UserId).Returns((string?)null);
        var command = new CreateCommand(Currencies.THB);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _requestContextMock.SetupGet(r => r.UserId).Returns("999");
        var command = new CreateCommand(Currencies.THB);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("User not found", exception.Message);
    }
}
