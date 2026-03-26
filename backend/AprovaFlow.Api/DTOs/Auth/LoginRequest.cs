using FluentValidation;

namespace AprovaFlow.Api.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A password é obrigatória.");
    }
}
