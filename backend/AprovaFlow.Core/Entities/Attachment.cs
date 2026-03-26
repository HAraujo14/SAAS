namespace AprovaFlow.Core.Entities;

/// <summary>
/// Ficheiro anexado a um pedido.
/// StoragePath contém o caminho relativo no sistema de ficheiros ou a chave no S3.
/// O download é feito via endpoint autenticado — nunca exposição directa de path.
/// </summary>
public class Attachment : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid UploadedById { get; set; }
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Caminho interno de armazenamento.
    /// Local: "uploads/{tenantId}/{year}/{month}/{guid}.ext"
    /// S3: "tenants/{tenantId}/attachments/{guid}.ext"
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navegação
    public Request Request { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
