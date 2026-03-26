using FluentValidation;

namespace AprovaFlow.Api.DTOs.Approvals;

/// <summary>
/// Body enviado ao aprovar ou rejeitar um pedido.
/// Comment é obrigatório apenas em Reject (validado no serviço).
/// </summary>
public record ApprovalActionRequest(string? Comment);

public class ApprovalActionRequestValidator : AbstractValidator<ApprovalActionRequest>
{
    public ApprovalActionRequestValidator()
    {
        // Sem regras globais aqui — a obrigatoriedade do comentário
        // é contextual (apenas para rejeição) e validada no serviço.
        RuleFor(x => x.Comment)
            .MaximumLength(2000).When(x => x.Comment is not null);
    }
}

public record ApprovalHistoryDto(
    Guid Id,
    string StepLabel,
    int StepOrder,
    string ApproverName,
    string? Decision,
    string? Comment,
    DateTime? DecidedAt,
    DateTime CreatedAt
);
