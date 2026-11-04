using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Auth;
using TaskTracker.Application.Auth.Login;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenDto>> Login(
        LoginCommand command,
        [FromServices] ICommandHandler<LoginCommand, AccessTokenDto> handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(command, cancellationToken));
    }
}
