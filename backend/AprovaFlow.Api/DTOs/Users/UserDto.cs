using FluentValidation;

namespace AprovaFlow.Api.DTOs.Users;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt
);

public record CreateUserRequest(string Name, string Email, string Password, string Role);

public record UpdateUserRequest(string Name, string Role, bool IsActive);

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    private static readonly string[] ValidRoles = ["Collaborator", "Approver", "Admin"];

    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Deve conter uma letra maiúscula.")
            .Matches(@"[0-9]").WithMessage("Deve conter um número.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => ValidRoles.Contains(r))
            .WithMessage($"Papel inválido. Valores aceites: {string.Join(", ", ValidRoles)}");
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    private static readonly string[] ValidRoles = ["Collaborator", "Approver", "Admin"];

    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role)
            .Must(r => ValidRoles.Contains(r))
            .WithMessage($"Papel inválido. Valores aceites: {string.Join(", ", ValidRoles)}");
    }
}
