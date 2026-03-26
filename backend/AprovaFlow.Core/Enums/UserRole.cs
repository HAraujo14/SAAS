namespace AprovaFlow.Core.Enums;

/// <summary>
/// Define o papel do utilizador dentro do tenant.
/// Collaborator: cria pedidos.
/// Approver: aprova/rejeita pedidos atribuídos.
/// Admin: gere utilizadores, tipos de pedido e configurações do tenant.
/// </summary>
public enum UserRole
{
    Collaborator = 1,
    Approver = 2,
    Admin = 3
}
