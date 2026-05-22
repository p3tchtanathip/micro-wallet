using FluentValidation;

namespace Application.Wallets.Commands.AiQuery;

public class AiQueryCommandValidator : AbstractValidator<AiQueryCommand>
{
    public AiQueryCommandValidator()
    {
        RuleFor(x => x.WalletId).GreaterThan(0);
        RuleFor(x => x.Query).MinimumLength(2).MaximumLength(300).WithMessage("Query must not exceed 300 characters.");
    }
}