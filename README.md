# Task Tracker — Backend

[![CI](https://github.com/joakososa/task-tracker-back-end/actions/workflows/ci.yml/badge.svg)](https://github.com/joakososa/task-tracker-back-end/actions/workflows/ci.yml)

A Trello-like task management API built from scratch in .NET, used as a hands-on
project to practice **backend architecture, testing, CI/CD and Azure deployment**.

The application itself is deliberately simple (users, projects, tasks, comments).
The focus is on everything around it: how it is structured, tested, built,
deployed and operated.

## Goals

- [x] Design the API contract first (OpenAPI) from written functional requirements
- [x] Clean Architecture solution layout enforced by project references
- [x] Domain model with unit tests (xUnit)
- [x] EF Core + SQL Server with migrations, Fluent API configurations and audit fields
- [x] JWT authentication (register, login, current user profile)
- [ ] Resource-based authorization (project membership)
- [x] Integration tests against a real SQL Server using Testcontainers
- [x] CI pipeline on GitHub Actions (build, format, tests, coverage, dependency review)
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
| CI/CD | GitHub Actions (CI; CD *planned*) |
| Cloud | Azure *(planned)* |

## Running locally

Prerequisites: .NET 9 SDK, Docker.

```bash
docker compose up -d --wait                            # SQL Server 2022 on localhost:1433
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)" --project src/TaskTracker.Api   # once
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

## Continuous integration

Every pull request to `main` (and every push to it) runs [`ci.yml`](.github/workflows/ci.yml):

1. **Format:** `dotnet format --verify-no-changes`, with the rules in `.editorconfig`.
2. **Build** in Release with warnings as errors (including Microsoft's recommended analyzers).
3. **Unit and integration tests**, the latter against SQL Server in the runner's Docker.
4. **Coverage report** in the run summary, full HTML as an artifact.
5. **Dependency review:** the PR fails if it adds a package with a known vulnerability.

To reproduce a failing build or format check locally:

```bash
dotnet format                                          # fixes formatting
dotnet build -c Release -p:TreatWarningsAsErrors=true  # same build as CI
```

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
- **Domain errors carry a code and a kind** (`Validation`, `Conflict`, `NotFound`,
  `Forbidden`). The code matches `Error.code` in the OpenAPI contract; the API maps
  the kind to an HTTP status, so Domain knows nothing about HTTP (ADR 0007).
- **Missing fields are reported by the validator, not by model binding.**
  `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` stops ASP.NET Core
  from treating non-nullable strings as `[Required]`, so a missing `name` comes back
  together with every other field error (ADR 0007). Model binding only rejects
  malformed JSON and wrong types.
- **HTTP tests check responses against the contract.** Besides asserting the status,
  each test validates the response against `docs/openapi.yaml` (operation, documented
  status, body schema) with NSwag + NJsonSchema, so the contract cannot silently drift
  from the implementation.
- **`User` hashes its own password.** It receives the plain password and a Domain
  `IPasswordHasher` (implemented in Infrastructure with Identity's PBKDF2 hasher), so
  the password policy is an entity rule and no use case can skip it (ADR 0008).
- **Secure by default.** An authorization fallback policy requires a valid token on
  every endpoint; the public ones (register, login, `/health`) opt out explicitly
  with `[AllowAnonymous]`. Forgetting an attribute closes an endpoint instead of
  opening it.
- **Every 401 uses the contract's `Error` shape.** JwtBearer answers a rejected
  token with an empty body, so its challenge is overridden to write the same
  `ErrorResponse` as every other error. Two codes: `INVALID_CREDENTIALS` for a failed
  login, `UNAUTHORIZED` for a missing or invalid token.
- **Failed logins are an Application concern.** Authenticating is a use case, not an
  entity rule, so it throws an Application exception instead of a domain error. Unknown
  email and wrong password give the same response; response time is not equalized,
  since registration already reveals whether an email exists (409).
- **Short-lived HS256 tokens, key outside the repo.** Tokens last 60 minutes with a
  30-second clock skew (the default is 5 minutes, meant for many servers). The signing
  key comes from user-secrets locally and is validated at startup.
- **Enums in SQL Server, two ways on purpose.** `TaskState` is a lookup table
  seeded from the enum (FK-enforced); `TaskPriority` is a string column with a
  `CHECK` constraint. Both are valid; having both makes the trade-off visible.
- **`DateTimeOffset` everywhere.** Removes the `DateTime.Kind` ambiguity at the
  cost of two bytes per column.
- **Physical deletes with cascades.** Soft delete is out of scope for the MVP.
- **Exact versions, each in one place.** The SDK is pinned in `global.json`, every
  package version lives in `Directory.Packages.props` (Central Package Management),
  and the SQL Server image uses an exact tag. An upgrade is a reviewed change, never
  a surprise in a build.
- **Warnings fail the build only in CI.** Locally they show up but never block you
  while working; nothing with warnings reaches `main`. The CI failure says so
  explicitly. NuGet audit warnings are the exception, see the next point.
- **New dependencies are reviewed; existing ones are alerted.** A PR that adds a
  vulnerable package fails (dependency review). A vulnerability published later for a
  package `main` already uses does not block unrelated PRs: it raises a Dependabot alert
  instead. GitHub cannot read versions from `Directory.Packages.props`, so CI submits
  the packages the restore actually resolved (direct and transitive) with Microsoft's
  component detection.
- **CI actions are pinned and the token is read-only.** Actions are referenced by
  commit SHA, since a tag can be moved to other code; Dependabot keeps the SHAs up to
  date. Jobs only get read access to the repository, except the one that submits
  dependencies.
- **Coverage is reported, not enforced.** A threshold rewards tests written for the
  number; the report is there to spot untested areas.

## Documentation

- [Functional requirements](docs/specs.md)
- [OpenAPI contract](docs/openapi.yaml)
- [Architecture Decision Records](docs/architecture/)
