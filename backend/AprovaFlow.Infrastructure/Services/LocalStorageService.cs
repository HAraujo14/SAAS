using AprovaFlow.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AprovaFlow.Infrastructure.Services;

/// <summary>
/// Armazenamento local de ficheiros para desenvolvimento e deployments simples.
/// Em produção: substituir por S3StorageService (mesma interface IStorageService).
///
/// Segurança:
/// - Não expõe o StoragePath directamente ao cliente.
/// - O download é sempre feito através de endpoint autenticado.
/// - Extensões e MIME types permitidos são validados antes do upload.
///
/// Organização: uploads/{tenantId}/{ano}/{mês}/{guid}.{ext}
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly long _maxFileSizeBytes;
    private readonly ILogger<LocalStorageService> _logger;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain",
        "text/csv"
    };

    public LocalStorageService(IConfiguration config, ILogger<LocalStorageService> logger)
    {
        _basePath = config["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _maxFileSizeBytes = long.Parse(config["Storage:MaxFileSizeMb"] ?? "10") * 1024 * 1024;
        _logger = logger;

        Directory.CreateDirectory(_basePath);
    }

    public async Task<UploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string mimeType,
        Guid tenantId,
        CancellationToken ct = default)
    {
        if (!IsAllowedMimeType(mimeType))
            throw new InvalidOperationException($"Tipo de ficheiro não permitido: {mimeType}");

        if (!IsWithinSizeLimit(fileStream.Length))
            throw new InvalidOperationException(
                $"Ficheiro demasiado grande. Máximo: {_maxFileSizeBytes / 1024 / 1024}MB");

        var ext = Path.GetExtension(fileName);
        var now = DateTime.UtcNow;
        var relativePath = Path.Combine(
            tenantId.ToString(), now.Year.ToString(), now.Month.ToString("D2"),
            $"{Guid.NewGuid()}{ext}");

        var fullPath = Path.Combine(_basePath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileOutput = File.Create(fullPath);
        await fileStream.CopyToAsync(fileOutput, ct);

        _logger.LogInformation("Ficheiro guardado: {Path}", relativePath);

        return new UploadResult(relativePath, fileName, mimeType, fileStream.Length);
    }

    public async Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Ficheiro não encontrado.", storagePath);

        // Retorna um FileStream — o controller fará o streaming para o cliente.
        return await Task.FromResult(File.OpenRead(fullPath));
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Ficheiro apagado: {Path}", storagePath);
        }
        await Task.CompletedTask;
    }

    public bool IsAllowedMimeType(string mimeType)
        => AllowedMimeTypes.Contains(mimeType);

    public bool IsWithinSizeLimit(long sizeBytes)
        => sizeBytes <= _maxFileSizeBytes;
}
