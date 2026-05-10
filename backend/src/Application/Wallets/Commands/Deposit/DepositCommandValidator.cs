using FluentValidation;

namespace Application.Wallets.Commands.Deposit;

public class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(x => x.WalletId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
    }
}