using FluentValidation;

namespace AprovaFlow.Api.DTOs.Auth;

/// <summary>
/// Registo de um novo tenant + utilizador administrador.
/// Um único endpoint cria a empresa e o primeiro admin simultaneamente.
/// </summary>
public record RegisterRequest(
    string TenantName,
    string TenantSlug,
    string AdminName,
    string AdminEmail,
    string AdminPassword
);

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.TenantSlug)
            .NotEmpty().WithMessage("O slug da empresa é obrigatório.")
            .MaximumLength(100)
            .Matches(@"^[a-z0-9\-]+$").WithMessage("O slug só pode conter letras minúsculas, números e hífenes.");

        RuleFor(x => x.AdminName)
            .NotEmpty().WithMessage("O nome do administrador é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("A password é obrigatória.")
            .MinimumLength(8).WithMessage("A password deve ter pelo menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("A password deve conter pelo menos uma letra maiúscula.")
            .Matches(@"[0-9]").WithMessage("A password deve conter pelo menos um número.");
    }
}
