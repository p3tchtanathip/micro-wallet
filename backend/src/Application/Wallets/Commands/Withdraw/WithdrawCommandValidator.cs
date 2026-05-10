using FluentValidation;

namespace Application.Wallets.Commands.Withdraw;

public class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(x => x.WalletId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");
    }
}