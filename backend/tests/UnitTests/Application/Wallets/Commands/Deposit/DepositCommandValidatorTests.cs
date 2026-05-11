using Application.Wallets.Commands.Deposit;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Wallets.Commands.Deposit;

public class DepositCommandValidatorTests
{
    private readonly DepositCommandValidator _validator;

    public DepositCommandValidatorTests()
    {
        _validator = new DepositCommandValidator();
    }

    // WalletId
    [Fact]
    public void Validate_WithValidWalletId_ShouldNotHaveError()
    {
        var command = new DepositCommand(1, 100);
        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.WalletId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Validate_WithInvalidWalletId_ShouldHaveError(long walletId)
    {
        var command = new DepositCommand(walletId, 100);
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.WalletId);
    }

    // Amount
    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(999999)]
    public void Validate_WithValidAmount_ShouldNotHaveError(decimal amount)
    {
        var command = new DepositCommand(1, amount);
        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Validate_WithInvalidAmount_ShouldHaveError(decimal amount)
    {
        var command = new DepositCommand(1, amount);
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    // Description
    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveError()
    {
        var command = new DepositCommand(1, 100, null);
        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithDescriptionAt200Characters_ShouldNotHaveError()
    {
        var command = new DepositCommand(1, 100, new string('a', 200));
        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithDescriptionExceeding200Characters_ShouldHaveError()
    {
        var command = new DepositCommand(1, 100, new string('a', 201));
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 200 characters.");
    }
}
