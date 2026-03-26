using FluentValidation;

namespace AprovaFlow.Api.DTOs.Requests;

// ─── Criação de Pedido ───────────────────────────────────────────────────────

/// <summary>
/// Body do POST /api/requests.
/// FieldValues é um dicionário {requestFieldId → valor}, permitindo
/// ao frontend enviar os campos dinâmicos sem conhecer a estrutura interna.
/// </summary>
public record CreateRequestRequest(
    Guid RequestTypeId,
    string Title,
    string? Description,
    Dictionary<Guid, string> FieldValues
);

public class CreateRequestRequestValidator : AbstractValidator<CreateRequestRequest>
{
    public CreateRequestRequestValidator()
    {
        RuleFor(x => x.RequestTypeId).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título do pedido é obrigatório.")
            .MaximumLength(200);
        RuleFor(x => x.FieldValues).NotNull();
    }
}

// ─── Listagem de Pedidos ─────────────────────────────────────────────────────

public record RequestListDto(
    Guid Id,
    string Title,
    string Status,
    string RequestTypeName,
    string RequestTypeIcon,
    string RequesterName,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ResolvedAt
);

// ─── Detalhe de Pedido ───────────────────────────────────────────────────────

public record RequestDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    RequestTypeInfoDto RequestType,
    UserSummaryDto Requester,
    List<FieldValueDto> FieldValues,
    List<ApprovalSummaryDto> Approvals,
    List<CommentDto> Comments,
    List<AttachmentDto> Attachments,
    CurrentStepDto? CurrentStep,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ResolvedAt
);

public record RequestTypeInfoDto(Guid Id, string Name, string Icon);
public record UserSummaryDto(Guid Id, string Name, string Email);

public record FieldValueDto(
    Guid FieldId,
    string FieldLabel,
    string FieldType,
    string Value
);

public record ApprovalSummaryDto(
    Guid Id,
    string StepLabel,
    int StepOrder,
    UserSummaryDto Approver,
    string? Decision,
    string? Comment,
    DateTime? DecidedAt
);

public record CommentDto(
    Guid Id,
    UserSummaryDto Author,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsEdited
);

public record AttachmentDto(
    Guid Id,
    string FileName,
    string MimeType,
    long SizeBytes,
    UserSummaryDto UploadedBy,
    DateTime CreatedAt
);

public record CurrentStepDto(Guid StepId, string Label, int StepOrder);

// ─── Parâmetros de Query para Listagem ──────────────────────────────────────

public record RequestQueryParams(
    string? Status,
    Guid? RequestTypeId,
    bool? MyRequests,
    int Page = 1,
    int PageSize = 20
);

// ─── Resposta Paginada ───────────────────────────────────────────────────────

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
)
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
