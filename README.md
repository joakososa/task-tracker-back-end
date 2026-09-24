# Task Tracker — Backend

A Trello-like task management API built from scratch in .NET, used as a hands-on
project to practice **backend architecture, testing, CI/CD and Azure deployment**.

The application itself is deliberately simple (users, projects, tasks, comments).
The focus is on everything around it: how it is structured, tested, built,
deployed and operated.

## Goals

- [x] Design the API contract first (OpenAPI) from written functional requirements
- [x] Clean Architecture solution layout enforced by project references
- [x] Domain model with unit tests (xUnit)
- [ ] EF Core + SQL Server with migrations, Fluent API configurations and audit fields
- [ ] JWT authentication and resource-based authorization (project membership)
- [ ] Integration tests against a real SQL Server using Testcontainers
- [ ] CI pipeline (build, test, coverage) on GitHub Actions
- [ ] Containerized API (Docker) and CD pipeline
- [ ] Deployment to Azure with infrastructure as code (AWS later, as a separate exercise)
- [ ] Caching layer for project membership and project reads
- [ ] Stored procedures for reporting
- [ ] Extract a reporting microservice (Python) communicating via events
- [ ] Serverless component (Azure Functions) and Cosmos DB usage

## Architecture

```
src/
  TaskTracker.Domain            Entities, enums, business rules. No dependencies.
  TaskTracker.Application       Use cases, DTOs, interfaces (ports). Depends on Domain.
  TaskTracker.Infrastructure    EF Core, SQL Server, JWT. Implements Application interfaces.
  TaskTracker.Api               ASP.NET Core controllers, DI composition root, middleware.
tests/
  TaskTracker.UnitTests         Domain and Application tests. No I/O.
  TaskTracker.IntegrationTests  Full HTTP tests against SQL Server in Docker (Testcontainers).
docs/
  specs.md                      Functional requirements
  openapi.yaml                  API contract
  architecture/                 Architecture Decision Records (ADRs)
```

Dependencies point inwards only (Api → Infrastructure → Application → Domain).
The compiler enforces it: Domain cannot reference EF Core even by accident.

## Tech stack

| Area | Choice |
|---|---|
| Runtime | .NET 9 / C# 13 |
| Web | ASP.NET Core Web API (controllers) |
| Data | Entity Framework Core 9, SQL Server 2022 |
| Auth | JWT bearer tokens, ASP.NET Core `PasswordHasher` |
| Testing | xUnit, Testcontainers |
| Local infra | Docker Compose |
| CI/CD | GitHub Actions *(planned)* |
| Cloud | Azure *(planned)* |

## Running locally

Prerequisites: .NET 9 SDK, Docker.

```bash
docker compose up -d                                   # SQL Server 2022 on localhost:1433
dotnet run --project src/TaskTracker.Api --launch-profile http
curl http://localhost:5000/health                      # → Healthy
```

The OpenAPI document is served at `/openapi/v1.json` in Development.

## Testing

```bash
dotnet test                                            # unit + integration
dotnet test tests/TaskTracker.UnitTests                # unit only, no Docker needed
```

Integration tests start an ephemeral SQL Server container per run via Testcontainers.

## Design decisions

Significant decisions are recorded as ADRs in [`docs/architecture/`](docs/architecture/).
Smaller, non-obvious choices are noted here until they deserve one.

- **Api is both presentation and composition root.** `Program.cs` lives in the Api
  project because something has to know every layer to wire them. Split into a
  separate Host project only if a second host (e.g. a worker) appears.
- **Own `Users` table instead of ASP.NET Identity.** Keeps the focus on the
  learning goals; hashing uses Identity's own `PasswordHasher`, so migrating
  later is straightforward.
- **Rich domain entities.** Rules such as task state transitions and project
  membership live in the entities and are covered by unit tests; use cases
  orchestrate, they do not decide.
- **Domain errors carry a code**, matching the `Error.code` in the OpenAPI
  contract. The API layer maps codes to HTTP status; Domain knows nothing about HTTP.
- **Enums in SQL Server, two ways on purpose.** `TaskState` is a lookup table
  seeded from the enum (FK-enforced); `TaskPriority` is a string column with a
  `CHECK` constraint. Both are valid; having both makes the trade-off visible.
- **`DateTimeOffset` everywhere.** Removes the `DateTime.Kind` ambiguity at the
  cost of two bytes per column.
- **Physical deletes with cascades.** Soft delete is out of scope for the MVP.

## Documentation

- [Functional requirements](docs/specs.md)
- [OpenAPI contract](docs/openapi.yaml)
- [Architecture Decision Records](docs/architecture/)
