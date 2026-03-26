using AprovaFlow.Api.DTOs.Users;
using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using AprovaFlow.Core.Exceptions;
using AprovaFlow.Core.Interfaces.Repositories;
using AprovaFlow.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprovaFlow.Api.Controllers;

/// <summary>
/// Gestão de utilizadores dentro do tenant.
/// Todas as operações de escrita requerem papel Admin.
/// Leitura (GET) está disponível para todos os utilizadores autenticados do tenant.
/// </summary>
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista todos os utilizadores do tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var users = await _uow.Users.GetByTenantAsync(_currentUser.TenantId, includeInactive, ct);
        return Ok(users.Select(MapToDto));
    }

    /// <summary>
    /// Detalhe de um utilizador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await GetTenantUserAsync(id, ct);
        return Ok(MapToDto(user));
    }

    /// <summary>
    /// Cria novo utilizador no tenant. Apenas Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var emailExists = await _uow.Users.ExistsAsync(
            u => u.Email == request.Email.ToLowerInvariant() &&
                 u.TenantId == _currentUser.TenantId &&
                 u.DeletedAt == null, ct);

        if (emailExists)
            throw new ConflictException($"Já existe um utilizador com o email '{request.Email}'.");

        var role = Enum.Parse<UserRole>(request.Role);

        var user = new User
        {
            TenantId = _currentUser.TenantId,
            Name = request.Name,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Role = role
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user));
    }

    /// <summary>
    /// Actualiza nome, papel e estado activo. Apenas Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var user = await GetTenantUserAsync(id, ct);

        // Admin não pode alterar o próprio papel (evitar auto-despromoção)
        if (user.Id == _currentUser.UserId && request.Role != "Admin")
            throw new DomainException("Não pode alterar o seu próprio papel de administrador.");

        user.Name = request.Name;
        user.Role = Enum.Parse<UserRole>(request.Role);
        user.IsActive = request.IsActive;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return Ok(MapToDto(user));
    }

    /// <summary>
    /// Desactiva utilizador (soft delete). Apenas Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var user = await GetTenantUserAsync(id, ct);

        if (user.Id == _currentUser.UserId)
            throw new DomainException("Não pode eliminar a sua própria conta.");

        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }

    // ─── Me ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Perfil do utilizador autenticado.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(_currentUser.UserId, ct)
            ?? throw new NotFoundException("User", _currentUser.UserId);
        return Ok(MapToDto(user));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<User> GetTenantUserAsync(Guid id, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user is null || user.TenantId != _currentUser.TenantId || user.DeletedAt is not null)
            throw new NotFoundException("User", id);
        return user;
    }

    private static UserDto MapToDto(User u) => new(
        u.Id, u.Name, u.Email, u.Role.ToString(), u.IsActive, u.LastLoginAt, u.CreatedAt);
}
