using FluentValidation;

namespace Application.Wallets.Commands.Deposit;

public class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(x => x.WalletId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}