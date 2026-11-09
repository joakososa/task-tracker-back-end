# 0009. Project authorization: per-project roles stored with the membership, checked in each use case

* **State:** Accepted
* **Date:** 2026-11-08
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code

## Context
From step 6 on, almost every endpoint acts on a project or on something inside one (tasks, comments).
RF-05, RF-07 and RF-08 define who may do what: members see the project and work on its tasks; only the
owner edits or deletes the project and adds members. RF-18 asks for 404 when a resource does not exist and
403 when the user is not a member.

The schema stored ownership in `Projects.OwnerId` and membership in `ProjectMembers`, so "what can this user
do here" needed both. Routes identify the resource differently: `/projects/{id}` carries the project id,
`/tasks/{id}` and `/tasks/{id}/comments` carry a task id.

## Decision
* **Roles are per project and live in the database**, in a `Role` column of `ProjectMembers`
  (`Owner` / `Member`, string + CHECK like `TaskPriority`). The owner is a membership row; `Projects.OwnerId`
  is removed. A filtered unique index (`WHERE Role = 'Owner'`) guarantees exactly one owner per project.
* **Roles map to permissions in Domain** (`ProjectRole` → `ProjectPermission`). Use cases ask for a
  permission (`EditProject`, `ManageMembers`, `ManageTasks`, ...), never for a role, so a new role only
  changes the map.
* **Load, then authorize, inside each handler.** The handler loads the resource it needs anyway, takes its
  `ProjectId` and checks the permission before changing anything:
  * Project use cases authorize in memory from the loaded aggregate (`project.RoleOf(userId)`).
  * Task and comment use cases call an Application service (`EnsureAsync(projectId, permission)`) backed by
    `IProjectRepository.GetRoleAsync(projectId, userId)`: one index seek on the `ProjectMembers` primary key.
* **Status codes follow RF-18:** missing resource → 404 (`NotFoundException`), existing resource without the
  permission → 403 (`ForbiddenException`); both Application exceptions mapped by `ErrorMapper` (ADR 0007).
* **Validation runs before authorization** (the `ValidationDecorator` wraps the handler). A non-member sending
  an invalid body gets 400; the validation rules are already public in `docs/openapi.yaml`.

### Pros
* **Authorization is in Application**, so any future entry point (Azure Function, another service) gets it.
* **No chains of lookups:** every aggregate knows its `ProjectId`, so the check is always "project + user → role".
* **One cheap query** for task and comment endpoints, none extra for project endpoints; the role lookup is the
  natural target for the membership cache in the roadmap.
* **One source of truth for ownership**, with the "one owner" rule enforced by the database.

### Cons
* **Each handler must call the check explicitly.** Forgetting it leaves the endpoint open to any authenticated
  user, and nothing fails at build time. The safety net is an HTTP test per endpoint with a non-member user.
* A schema migration (move owners into `ProjectMembers`, drop `Projects.OwnerId`).
* `ProjectResponse.owner` is now derived from the membership rows.

## Alternatives considered
* **An authorization decorator** (commands implement `IProjectRequest { ProjectId, RequiredRole }`, a
  decorator checks before the handler). Uniform, but routes with a task id need a task → project lookup and
  future comment routes a comment → task → project one; the resource is loaded twice; and forgetting the
  interface fails silently too. A reflection test to enforce it would guess at a domain that may still change.
  Rejected as impractical.
* **ASP.NET Core resource-based authorization (policies).** Idiomatic .NET, but it lives in Api: use cases
  would be unprotected from other entry points, the route id still has to be resolved to a project, and a
  missing attribute fails silently. Rejected.
* **Roles as JWT claims.** Roles depend on the project and change when members are added or removed; claims
  would be stale until the token expires and grow with every project. Rejected.
* **Keep `Projects.OwnerId` and a separate membership table.** No migration, but two places to check and no
  room for more roles. Rejected.
* **404 for non-members too** (hides which ids exist). Contradicts RF-18; existence leaks are accepted here
  (`POST /users` already reveals emails). Rejected.
