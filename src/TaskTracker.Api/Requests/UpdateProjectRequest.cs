namespace TaskTracker.Api.Requests;

/// The PUT /projects/{id} body (contract ProjectEditInput). The id comes from the route, so it is not here.
public sealed record UpdateProjectRequest(string Name);
