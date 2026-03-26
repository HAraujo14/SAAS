using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AprovaFlow.Infrastructure.Services;

/// <summary>
/// Implementação de notificações por email usando MailKit.
/// Configuração via appsettings:
///   Email:Host, Email:Port, Email:Username, Email:Password, Email:FromAddress, Email:FromName
///
/// Para desenvolvimento: usar MailHog (smtp:1025, sem autenticação).
/// Para produção: configurar SendGrid, SES ou SMTP próprio.
///
/// Todos os envios são fire-and-forget em relação à transacção principal
/// (não devem falhar o pedido se o email não for entregue).
/// </summary>
public class EmailNotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IUnitOfWork uow,
        IConfiguration config,
        ILogger<EmailNotificationService> logger)
    {
        _uow = uow;
        _config = config;
        _logger = logger;
    }

    public async Task SendRequestSubmittedAsync(Guid requestId, CancellationToken ct = default)
    {
        try
        {
            var request = await _uow.Requests.GetDetailAsync(requestId, Guid.Empty, ct);
            if (request is null) return;

            await SendEmailAsync(
                to: request.Requester.Email,
                toName: request.Requester.Name,
                subject: $"[AprovaFlow] Pedido submetido: {request.Title}",
                body: BuildSubmittedBody(request.Requester.Name, request.Title),
                ct: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de pedido submetido para requestId {RequestId}", requestId);
        }
    }

    public async Task SendApprovalRequiredAsync(Guid requestId, Guid approverId, CancellationToken ct = default)
    {
        try
        {
            var approver = await _uow.Users.GetByIdAsync(approverId, ct);
            var request = await _uow.Requests.GetDetailAsync(requestId, Guid.Empty, ct);
            if (approver is null || request is null) return;

            await SendEmailAsync(
                to: approver.Email,
                toName: approver.Name,
                subject: $"[AprovaFlow] Aprovação necessária: {request.Title}",
                body: BuildApprovalRequiredBody(approver.Name, request.Title, request.Requester.Name),
                ct: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de aprovação necessária para requestId {RequestId}", requestId);
        }
    }

    public async Task SendRequestApprovedAsync(Guid requestId, CancellationToken ct = default)
    {
        try
        {
            var request = await _uow.Requests.GetDetailAsync(requestId, Guid.Empty, ct);
            if (request is null) return;

            await SendEmailAsync(
                to: request.Requester.Email,
                toName: request.Requester.Name,
                subject: $"[AprovaFlow] Pedido aprovado: {request.Title}",
                body: BuildApprovedBody(request.Requester.Name, request.Title),
                ct: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de aprovação para requestId {RequestId}", requestId);
        }
    }

    public async Task SendRequestRejectedAsync(Guid requestId, string rejectionReason, CancellationToken ct = default)
    {
        try
        {
            var request = await _uow.Requests.GetDetailAsync(requestId, Guid.Empty, ct);
            if (request is null) return;

            await SendEmailAsync(
                to: request.Requester.Email,
                toName: request.Requester.Name,
                subject: $"[AprovaFlow] Pedido rejeitado: {request.Title}",
                body: BuildRejectedBody(request.Requester.Name, request.Title, rejectionReason),
                ct: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de rejeição para requestId {RequestId}", requestId);
        }
    }

    public async Task SendCommentAddedAsync(Guid requestId, Guid commentAuthorId, CancellationToken ct = default)
    {
        // Notifica todos os participantes do pedido excepto o autor do comentário
        try
        {
            var request = await _uow.Requests.GetDetailAsync(requestId, Guid.Empty, ct);
            if (request is null) return;

            var participants = new HashSet<string>();
            participants.Add(request.Requester.Email);
            foreach (var approval in request.Approvals)
                participants.Add(approval.Approver.Email);

            var author = await _uow.Users.GetByIdAsync(commentAuthorId, ct);
            if (author is not null) participants.Remove(author.Email);

            foreach (var email in participants)
            {
                await SendEmailAsync(
                    to: email,
                    toName: email,
                    subject: $"[AprovaFlow] Novo comentário: {request.Title}",
                    body: BuildCommentBody(request.Title, author?.Name ?? "Alguém"),
                    ct: ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email de comentário para requestId {RequestId}", requestId);
        }
    }

    // ─── Envio SMTP ──────────────────────────────────────────────────────────

    private async Task SendEmailAsync(
        string to, string toName, string subject, string body, CancellationToken ct)
    {
        var host = _config["Email:Host"] ?? "localhost";
        var port = int.Parse(_config["Email:Port"] ?? "1025");
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];
        var fromAddress = _config["Email:FromAddress"] ?? "noreply@aprovaflow.com";
        var fromName = _config["Email:FromName"] ?? "AprovaFlow";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.Auto, ct);

        if (!string.IsNullOrEmpty(username))
            await client.AuthenticateAsync(username, password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email enviado para {To}: {Subject}", to, subject);
    }

    // ─── Templates HTML simples ──────────────────────────────────────────────

    private static string BuildSubmittedBody(string name, string title) =>
        $"""
        <h2>Olá {name},</h2>
        <p>O seu pedido <strong>"{title}"</strong> foi submetido com sucesso e aguarda aprovação.</p>
        <p>Será notificado quando houver uma decisão.</p>
        <br/><p>AprovaFlow</p>
        """;

    private static string BuildApprovalRequiredBody(string approverName, string title, string requesterName) =>
        $"""
        <h2>Olá {approverName},</h2>
        <p>O utilizador <strong>{requesterName}</strong> submeteu o pedido <strong>"{title}"</strong> que requer a sua aprovação.</p>
        <p>Por favor aceda ao AprovaFlow para tomar uma decisão.</p>
        <br/><p>AprovaFlow</p>
        """;

    private static string BuildApprovedBody(string name, string title) =>
        $"""
        <h2>Olá {name},</h2>
        <p>O seu pedido <strong>"{title}"</strong> foi <span style="color:green"><strong>aprovado</strong></span>.</p>
        <br/><p>AprovaFlow</p>
        """;

    private static string BuildRejectedBody(string name, string title, string reason) =>
        $"""
        <h2>Olá {name},</h2>
        <p>O seu pedido <strong>"{title}"</strong> foi <span style="color:red"><strong>rejeitado</strong></span>.</p>
        <p><strong>Motivo:</strong> {reason}</p>
        <br/><p>AprovaFlow</p>
        """;

    private static string BuildCommentBody(string title, string authorName) =>
        $"""
        <h2>Novo comentário</h2>
        <p><strong>{authorName}</strong> comentou no pedido <strong>"{title}"</strong>.</p>
        <p>Aceda ao AprovaFlow para ver o comentário.</p>
        <br/><p>AprovaFlow</p>
        """;
}
