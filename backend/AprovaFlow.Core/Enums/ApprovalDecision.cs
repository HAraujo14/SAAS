namespace AprovaFlow.Core.Enums;

/// <summary>
/// Decisão tomada por um aprovador num dado step.
/// Approved  → step aprovado, avança para o próximo (ou conclui).
/// Rejected  → pedido rejeitado imediatamente.
/// Delegated → o aprovador transferiu a responsabilidade para outro utilizador.
/// </summary>
public enum ApprovalDecision
{
    Approved = 1,
    Rejected = 2,
    Delegated = 3
}
