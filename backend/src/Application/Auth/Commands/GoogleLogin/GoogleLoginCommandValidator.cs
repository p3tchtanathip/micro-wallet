using FluentValidation;

namespace Application.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID Token is required.")
            .Must(x => x.Split('.').Length == 3).WithMessage("Invalid Token format."); // JWT Structure (Header.Payload.Signature)
    }
}