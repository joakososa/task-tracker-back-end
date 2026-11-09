using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Projects;
using TaskTracker.Api.Requests;
using TaskTracker.Application.Projects.Create;
using TaskTracker.Application.Projects.Delete;
using TaskTracker.Application.Projects.GetById;
using TaskTracker.Application.Projects.List;
using TaskTracker.Application.Projects.Update;

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
        return CreatedAtAction(nameof(Get), new { id = project.Id }, project);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> List(
        [FromServices] IQueryHandler<ListProjectsQuery, IReadOnlyList<ProjectDto>> handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(new ListProjectsQuery(), cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Get(
        int id,
        [FromServices] IQueryHandler<GetProjectQuery, ProjectDto> handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(new GetProjectQuery(id), cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(
        int id,
        UpdateProjectRequest request,
        [FromServices] ICommandHandler<UpdateProjectCommand, ProjectDto> handler,
        CancellationToken cancellationToken)
    {
        return Ok(await handler.HandleAsync(new UpdateProjectCommand(id, request.Name), cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ICommandHandler<DeleteProjectCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeleteProjectCommand(id), cancellationToken);
        return NoContent();
    }
}
