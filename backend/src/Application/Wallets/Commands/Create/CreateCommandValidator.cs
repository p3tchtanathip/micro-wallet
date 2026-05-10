using Domain.Constants;
using FluentValidation;

namespace Application.Wallets.Commands.Create;

public class CreateCommandValidator : AbstractValidator<CreateCommand>
{
    public CreateCommandValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(c => Currencies.All.Contains(c))
            .WithMessage($"Currency must be one of: {string.Join(", ", Currencies.All)}");
    }
}
