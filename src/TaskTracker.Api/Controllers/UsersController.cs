using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Users;
using TaskTracker.Application.Users.Register;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    // 201 without a Location header: the contract has no GET /users/{id} to point to.
    [HttpPost]
    public async Task<ActionResult<UserDto>> Register(
        RegisterUserCommand command,
        [FromServices] ICommandHandler<RegisterUserCommand, UserDto> handler,
        CancellationToken cancellationToken)
    {
        var user = await handler.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, user);
    }
}
