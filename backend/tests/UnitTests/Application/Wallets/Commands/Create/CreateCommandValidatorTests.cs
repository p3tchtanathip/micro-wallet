using FluentValidation.TestHelper;
using Application.Wallets.Commands.Create;
using Domain.Constants;

namespace UnitTests.Application.Wallets.Commands.Create;

public class CreateCommandValidatorTests
{
    private readonly CreateCommandValidator _validator;

    public CreateCommandValidatorTests()
    {
        _validator = new CreateCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCurrency_ShouldNotHaveErrors()
    {
        var command = new CreateCommand(Currencies.THB);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(Currencies.THB)]
    [InlineData(Currencies.USD)]
    public void Validate_WithSupportedCurrency_ShouldNotHaveErrors(string currency)
    {
        var command = new CreateCommand(currency);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCurrency_ShouldHaveError()
    {
        var command = new CreateCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_WithEmptyCurrency_ShouldHaveCorrectErrorMessage()
    {
        var command = new CreateCommand(string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("'Currency' must not be empty.");
    }

    [Fact]
    public void Validate_WithUnsupportedCurrency_ShouldHaveError()
    {
        var command = new CreateCommand("EUR");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_WithUnsupportedCurrency_ShouldHaveCorrectErrorMessage()
    {
        var command = new CreateCommand("EUR");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage($"Currency must be one of: {string.Join(", ", Currencies.All)}");
    }

    [Fact]
    public void Validate_WithNullCurrency_ShouldHaveError()
    {
        var command = new CreateCommand(null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_WithLowercaseCurrency_ShouldHaveError()
    {
        var command = new CreateCommand("thb");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }
}