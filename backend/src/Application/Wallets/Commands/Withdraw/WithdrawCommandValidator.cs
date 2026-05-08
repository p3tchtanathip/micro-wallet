using FluentValidation;

namespace Application.Wallets.Commands.Withdraw;

public class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(x => x.WalletId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}