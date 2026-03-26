namespace AprovaFlow.Core.Interfaces.Services;

public record UploadResult(string StoragePath, string FileName, string MimeType, long SizeBytes);

/// <summary>
/// Abstracção de armazenamento de ficheiros.
/// Implementação local para desenvolvimento, S3/R2 para produção.
/// Os serviços de negócio são agnósticos ao provider de storage.
/// </summary>
public interface IStorageService
{
    Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string mimeType, Guid tenantId, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
    bool IsAllowedMimeType(string mimeType);
    bool IsWithinSizeLimit(long sizeBytes);
}
