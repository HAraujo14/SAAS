using AprovaFlow.Api.DTOs.Approvals;
using AprovaFlow.Api.DTOs.Comments;
using AprovaFlow.Api.DTOs.Requests;
using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Exceptions;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;
using AprovaFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AprovaFlow.Api.Controllers;

/// <summary>
/// CRUD de pedidos + acções de ciclo de vida (submit, cancel).
/// Comentários, anexos e aprovações têm os seus próprios controllers
/// para manter o código organizado, mas estão sob a rota /requests/{id}/...
/// </summary>
public class RequestsController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly RequestService _requestService;
    private readonly ApprovalService _approvalService;
    private readonly IStorageService _storageService;

    public RequestsController(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        RequestService requestService,
        ApprovalService approvalService,
        IStorageService storageService)
    {
        _uow = uow;
        _currentUser = currentUser;
        _requestService = requestService;
        _approvalService = approvalService;
        _storageService = storageService;
    }

    // ─── CRUD ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lista pedidos com paginação e filtros.
    /// </summary>
    /// <remarks>
    /// Parâmetros de query:
    /// - status: Draft | Pending | InReview | Approved | Rejected | Cancelled
    /// - requestTypeId: filtrar por tipo
    /// - myRequests: true = apenas os meus pedidos
    /// - page / pageSize
    ///
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "...",
    ///       "title": "Férias Janeiro",
    ///       "status": "Pending",
    ///       "requestTypeName": "Pedido de Férias",
    ///       "requestTypeIcon": "calendar",
    ///       "requesterName": "João Silva",
    ///       "createdAt": "2026-01-15T09:00:00Z"
    ///     }
    ///   ],
    ///   "totalCount": 42,
    ///   "page": 1,
    ///   "pageSize": 20,
    ///   "totalPages": 3,
    ///   "hasNextPage": true,
    ///   "hasPreviousPage": false
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RequestListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] Guid? requestTypeId,
        [FromQuery] bool myRequests = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        RequestStatus? statusEnum = null;
        if (status is not null && Enum.TryParse<RequestStatus>(status, out var s))
            statusEnum = s;

        var requesterId = myRequests ? _currentUser.UserId : (Guid?)null;

        // Collaborators só vêem os próprios pedidos
        if (_currentUser.Role == UserRole.Collaborator)
            requesterId = _currentUser.UserId;

        var (items, totalCount) = await _uow.Requests.GetPagedAsync(
            _currentUser.TenantId, requesterId, statusEnum, requestTypeId, page, pageSize, ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var result = new PagedResult<RequestListDto>(
            items.Select(MapToListDto).ToList(),
            totalCount, page, pageSize, totalPages);

        return Ok(result);
    }

    /// <summary>
    /// Detalhe completo de um pedido.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RequestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var request = await _uow.Requests.GetDetailAsync(id, _currentUser.TenantId, ct)
            ?? throw new NotFoundException("Request", id);

        // Collaborators só podem ver os próprios pedidos
        if (_currentUser.Role == UserRole.Collaborator && request.RequesterId != _currentUser.UserId)
            throw new ForbiddenException();

        return Ok(MapToDetailDto(request));
    }

    /// <summary>
    /// Cria pedido em estado Draft.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RequestDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRequestRequest request, CancellationToken ct)
    {
        var created = await _requestService.CreateAsync(
            request.RequestTypeId, request.Title, request.Description, request.FieldValues, ct);

        var detail = await _uow.Requests.GetDetailAsync(created.Id, _currentUser.TenantId, ct)!;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDetailDto(detail!));
    }

    /// <summary>
    /// Submete pedido Draft para aprovação.
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(RequestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var request = await _requestService.SubmitAsync(id, ct);
        var detail = await _uow.Requests.GetDetailAsync(request.Id, _currentUser.TenantId, ct);
        return Ok(MapToDetailDto(detail!));
    }

    /// <summary>
    /// Cancela pedido.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _requestService.CancelAsync(id, ct);
        return NoContent();
    }

    // ─── Aprovações ───────────────────────────────────────────────────────────

    /// <summary>
    /// Aprova o pedido no step actual.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApprovalActionRequest request, CancellationToken ct)
    {
        await _approvalService.ApproveAsync(id, request.Comment, ct);
        return NoContent();
    }

    /// <summary>
    /// Rejeita o pedido no step actual. Comentário obrigatório.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] ApprovalActionRequest request, CancellationToken ct)
    {
        await _approvalService.RejectAsync(id, request.Comment ?? string.Empty, ct);
        return NoContent();
    }

    // ─── Comentários ──────────────────────────────────────────────────────────

    /// <summary>
    /// Adiciona comentário ao pedido.
    /// </summary>
    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddComment(
        Guid id, [FromBody] CreateCommentRequest request, CancellationToken ct)
    {
        // Verificar acesso ao pedido
        var req = await _uow.Requests.GetDetailAsync(id, _currentUser.TenantId, ct)
            ?? throw new NotFoundException("Request", id);

        if (_currentUser.Role == UserRole.Collaborator && req.RequesterId != _currentUser.UserId)
            throw new ForbiddenException();

        var comment = new Comment
        {
            RequestId = id,
            AuthorId = _currentUser.UserId,
            Content = request.Content
        };

        await _uow.Comments.AddAsync(comment, ct);
        await _uow.SaveChangesAsync(ct);

        var author = await _uow.Users.GetByIdAsync(_currentUser.UserId, ct)!;
        return CreatedAtAction(nameof(GetById), new { id },
            new CommentDto(
                comment.Id,
                new(author!.Id, author.Name, author.Email),
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt,
                IsEdited: false));
    }

    // ─── Anexos ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload de ficheiro para um pedido. Multipart/form-data.
    /// </summary>
    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(50 * 1024 * 1024)]  // 50MB máximo por request
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadAttachment(
        Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Nenhum ficheiro enviado." });

        var req = await _uow.Requests.GetDetailAsync(id, _currentUser.TenantId, ct)
            ?? throw new NotFoundException("Request", id);

        if (!_storageService.IsAllowedMimeType(file.ContentType))
            throw new DomainException($"Tipo de ficheiro não permitido: {file.ContentType}");

        if (!_storageService.IsWithinSizeLimit(file.Length))
            throw new DomainException("Ficheiro excede o tamanho máximo permitido.");

        await using var stream = file.OpenReadStream();
        var uploadResult = await _storageService.UploadAsync(
            stream, file.FileName, file.ContentType, _currentUser.TenantId, ct);

        var attachment = new Attachment
        {
            RequestId = id,
            UploadedById = _currentUser.UserId,
            FileName = uploadResult.FileName,
            StoragePath = uploadResult.StoragePath,
            MimeType = uploadResult.MimeType,
            SizeBytes = uploadResult.SizeBytes
        };

        await _uow.Attachments.AddAsync(attachment, ct);
        await _uow.SaveChangesAsync(ct);

        var uploader = await _uow.Users.GetByIdAsync(_currentUser.UserId, ct)!;
        return CreatedAtAction(nameof(GetById), new { id },
            new AttachmentDto(
                attachment.Id,
                attachment.FileName,
                attachment.MimeType,
                attachment.SizeBytes,
                new(uploader!.Id, uploader.Name, uploader.Email),
                attachment.CreatedAt));
    }

    /// <summary>
    /// Download de anexo. Servia o ficheiro com o nome original.
    /// </summary>
    [HttpGet("attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid attachmentId, CancellationToken ct)
    {
        var attachment = await _uow.Attachments.GetByIdAsync(attachmentId, ct)
            ?? throw new NotFoundException("Attachment", attachmentId);

        // Verificar que pertence ao tenant
        var request = await _uow.Requests.GetDetailAsync(attachment.RequestId, _currentUser.TenantId, ct)
            ?? throw new ForbiddenException();

        var stream = await _storageService.DownloadAsync(attachment.StoragePath, ct);
        return File(stream, attachment.MimeType, attachment.FileName);
    }

    // ─── Mapeamentos ──────────────────────────────────────────────────────────

    private static RequestListDto MapToListDto(Request r) => new(
        r.Id, r.Title, r.Status.ToString(),
        r.RequestType.Name, r.RequestType.Icon,
        r.Requester.Name, r.CreatedAt, r.SubmittedAt, r.ResolvedAt);

    private static RequestDetailDto MapToDetailDto(Request r) => new(
        r.Id, r.Title, r.Description, r.Status.ToString(),
        new(r.RequestType.Id, r.RequestType.Name, r.RequestType.Icon),
        new(r.Requester.Id, r.Requester.Name, r.Requester.Email),
        r.FieldValues.Select(fv => new FieldValueDto(
            fv.RequestFieldId, fv.RequestField.Label,
            fv.RequestField.FieldType.ToString(), fv.Value)).ToList(),
        r.Approvals.Select(a => new ApprovalSummaryDto(
            a.Id, a.ApprovalStep.Label, a.ApprovalStep.StepOrder,
            new(a.Approver.Id, a.Approver.Name, a.Approver.Email),
            a.Decision?.ToString(), a.Comment, a.DecidedAt)).ToList(),
        r.Comments.Where(c => c.DeletedAt == null).Select(c => new CommentDto(
            c.Id, new(c.Author.Id, c.Author.Name, c.Author.Email),
            c.Content, c.CreatedAt, c.UpdatedAt,
            IsEdited: c.UpdatedAt > c.CreatedAt.AddSeconds(1))).ToList(),
        r.Attachments.Where(a => a.DeletedAt == null).Select(a => new AttachmentDto(
            a.Id, a.FileName, a.MimeType, a.SizeBytes,
            new(a.UploadedBy.Id, a.UploadedBy.Name, a.UploadedBy.Email),
            a.CreatedAt)).ToList(),
        r.CurrentStep is not null
            ? new(r.CurrentStep.Id, r.CurrentStep.Label, r.CurrentStep.StepOrder)
            : null,
        r.CreatedAt, r.SubmittedAt, r.ResolvedAt);
}
