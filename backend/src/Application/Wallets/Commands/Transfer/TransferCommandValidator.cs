using FluentValidation;

namespace Application.Wallets.Commands.Transfer;

public class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.FromWalletNumber)
            .NotEmpty().WithMessage("Source wallet number is required.")
            .MaximumLength(50).WithMessage("Source wallet number is too long.");

        RuleFor(x => x.ToWalletNumber)
            .NotEmpty().WithMessage("Destination wallet number is required.")
            .MaximumLength(50).WithMessage("Destination wallet number is too long.")
            .NotEqual(x => x.FromWalletNumber)
            .WithMessage("Source and destination wallets must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than 0.")
            .Must(x => x * 100 == Math.Floor(x * 100))
            .WithMessage("Amount cannot have more than 2 decimal places.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
    }
}
