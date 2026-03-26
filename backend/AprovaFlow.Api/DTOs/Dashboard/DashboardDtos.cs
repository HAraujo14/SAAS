namespace AprovaFlow.Api.DTOs.Dashboard;

/// <summary>
/// Dados do dashboard principal.
/// Adaptado conforme o papel do utilizador:
/// - Collaborator: vê apenas os seus próprios pedidos.
/// - Approver: vê pendentes de aprovação + resumo geral.
/// - Admin: vê tudo.
/// </summary>
public record DashboardSummaryDto(
    int TotalRequests,
    int PendingRequests,
    int ApprovedRequests,
    int RejectedRequests,
    int PendingMyApprovals,
    List<RecentRequestDto> RecentRequests
);

public record RecentRequestDto(
    Guid Id,
    string Title,
    string Status,
    string RequestTypeName,
    string RequesterName,
    DateTime UpdatedAt
);

public record DashboardStatsDto(
    int Draft,
    int Pending,
    int InReview,
    int Approved,
    int Rejected,
    int Cancelled
);
