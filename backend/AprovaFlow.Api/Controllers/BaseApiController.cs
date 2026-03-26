using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprovaFlow.Api.Controllers;

/// <summary>
/// Controller base. Todos os controllers herdam daqui.
/// Define a rota base e aplica [Authorize] globalmente
/// (endpoints públicos sobrepõem com [AllowAnonymous]).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
}
