using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Users;
using TaskTracker.Application.Users.GetCurrent;
using TaskTracker.Application.Users.Register;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    // 201 without a Location header: the contract has no GET /users/{id} to point to.
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(
        RegisterUserCommand command,
        [FromServices] ICommandHandler<RegisterUserCommand, UserDto> handler,
        CancellationToken cancellationToken)
    {
        var user = await handler.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrent(
        [FromServices] IQueryHandler<GetCurrentUserQuery, UserDto> handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(new GetCurrentUserQuery(), cancellationToken));
    }
}
