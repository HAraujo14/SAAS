namespace AprovaFlow.Core.Enums;

/// <summary>
/// Estados possíveis de um pedido ao longo do seu ciclo de vida.
/// Draft     → pedido criado mas não submetido (editável).
/// Pending   → submetido, aguarda primeira aprovação.
/// InReview  → está a ser processado em algum step intermédio.
/// Approved  → todos os steps aprovados, processo concluído.
/// Rejected  → rejeitado por qualquer aprovador em qualquer step.
/// Cancelled → cancelado pelo colaborador ou por um admin.
/// </summary>
public enum RequestStatus
{
    Draft = 1,
    Pending = 2,
    InReview = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6
}
