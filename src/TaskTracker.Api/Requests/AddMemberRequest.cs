namespace TaskTracker.Api.Requests;

/// The POST /projects/{id}/members body (contract AssociateIdInput). The project id comes from the route.
public sealed record AddMemberRequest(int UserId);
