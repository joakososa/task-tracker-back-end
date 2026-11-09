using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Projects;
using TaskTracker.Application.Projects.Create;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("projects")]
public sealed class ProjectsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(
        CreateProjectCommand command,
        [FromServices] ICommandHandler<CreateProjectCommand, ProjectDto> handler,
        CancellationToken cancellationToken)
    {
        var project = await handler.HandleAsync(command, cancellationToken);
        return Created($"/projects/{project.Id}", project);
    }
}
