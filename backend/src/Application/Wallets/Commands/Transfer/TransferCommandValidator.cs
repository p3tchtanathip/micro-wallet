using FluentValidation;

namespace Application.Wallets.Commands.Transfer;

public class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.FromWalletId)
            .NotEmpty().WithMessage("Source wallet ID is required.")
            .GreaterThan(0).WithMessage("Invalid source wallet ID.");

        RuleFor(x => x.ToWalletId)
            .NotEmpty().WithMessage("Destination wallet ID is required.")
            .GreaterThan(0).WithMessage("Invalid destination wallet ID.")
            .NotEqual(x => x.FromWalletId).WithMessage("Source and destination wallets must be different.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than 0.")
            .Must(x => x * 100 == Math.Floor(x * 100))
            .WithMessage("Amount cannot have more than 2 decimal places.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
    }
}
