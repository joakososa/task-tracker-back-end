# CLAUDE.md

Guidance for AI-assisted sessions in this repository. Humans: the README has the overview.

## What this is

A learning project: a Trello-like task API in .NET built from scratch to practice
backend architecture, testing, CI/CD and AWS. The app is deliberately simple; the
engineering around it is the point. Public repo, portfolio quality expected.

## Commands

```bash
docker compose up -d                                       # SQL Server 2022 (localhost:1433, sa / see compose)
dotnet build
dotnet test tests/TaskTracker.UnitTests                    # fast, no Docker
dotnet test                                                # includes integration tests (Docker required)
dotnet run --project src/TaskTracker.Api --launch-profile http   # http://localhost:5000, /health
dotnet ef migrations add <Name> --project src/TaskTracker.Infrastructure --startup-project src/TaskTracker.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/TaskTracker.Infrastructure --startup-project src/TaskTracker.Api
```

Inspect the DB: `docker exec tasktracker-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<pwd>" -C -d TaskTracker -Q "<sql>"`

## Architecture rules

- Clean Architecture; dependencies point inwards only: Api → Infrastructure → Application → Domain.
  Domain has **no** package references. Never add EF/ASP.NET to Domain or Application.
- Data access: one repository per aggregate root (no generic `IRepository<T>`), methods added only when a
  use case needs them, plus `IUnitOfWork.SaveChangesAsync()`. Interfaces in Application, EF implementations
  in Infrastructure. Domain entities are mapped directly by EF (no separate persistence models).
- Use cases: one handler per use case, commands and queries separated (light CQRS), no MediatR.
  Validation with FluentValidation on commands in Application; `IExceptionHandler` in Api maps codes → HTTP.
- Api is presentation **and** composition root (`Program.cs`). See README "Design decisions".
- Each layer registers its own services (`AddInfrastructure()`, `AddApplication()`); `Program.cs` only calls them.
- Domain entities are rich: constructors enforce invariants, `private set`, behaviour methods
  (`Project.AddMember`, `TaskItem.ChangeState`). Use `Guard.RequiredText/OptionalText` for text fields.
- Business rules that need data outside the entity (e.g. "assignee must be a project member")
  belong in Application use cases, not in the entity.
- Domain errors: throw `DomainException(code, message)` with a code from `DomainErrors`.
  Codes match `Error.code` in `docs/openapi.yaml`. Api maps code → HTTP status; Domain never knows HTTP.
- Persistence: one `IEntityTypeConfiguration<T>` per entity in `Infrastructure/Persistence/Configurations`.
  Nullability comes from the entity (no `IsRequired()`); max lengths come from Domain constants.
  `TaskState` is a seeded lookup table (`TaskStates`); `TaskPriority` is a string + CHECK. Both on purpose.
- `DateTimeOffset` for all timestamps. Physical deletes with cascades; `User` FKs are `Restrict`/`SetNull`.

## Conventions

- Code, commits, ADRs and README in English. `docs/specs.md` and OpenAPI descriptions may stay in Spanish.
- Entity is `TaskItem`, never `Task` (clashes with `System.Threading.Tasks.Task`).
- Tests: xUnit, plain `Assert`. Names `Method_Scenario_ExpectedResult` (say the result, not "Succeeds").
  Expected values are literals, never computed with the code under test.
  Domain/Application changes start with a failing test. One test per field for wiring; edge cases live
  where the logic lives (e.g. `GuardTests`).
- Unit tests never touch a DB. Integration tests use Testcontainers (real SQL Server), never InMemory/SQLite.
- Contract-first: `docs/openapi.yaml` is the source of truth for endpoints and DTOs. It may still be
  changed while it has no consumers; record why in the commit.
- Significant decisions get an ADR in `docs/architecture/` (Nygard template + "Alternatives considered").
  Smaller ones go in README "Design decisions".
- Package versions: `9.0.*` for Microsoft packages until Central Package Management is introduced with CI.

## Roadmap (from the project brief)

1. Solution from scratch in .NET ✅
2. Unit and integration tests (in progress)
3. SQL Server as primary DB ✅
4. CI/CD pipeline
5. Deploy to AWS (infrastructure as code; free plan, budget alarm first)
6. Cache (project membership + project reads, short TTL + explicit invalidation)
7. Split into / add a microservice (likely Python)
8. Service-to-service communication
9. Stored procedures (reporting)
10. DynamoDB
11. MongoDB
12. Something serverless

Priorities: SQL Server depth and CI/CD first. Functional scope: `docs/specs.md` (RF-01..RF-24).

## Current status

Step 3 (Domain + EF Core) **complete**:
- 3.1 Domain entities + unit tests ✅
- 3.2 EF configurations, `InitialCreate` migration, `docs/initial-schema.sql` ✅
- 3.3 Testcontainers fixture (`SqlServerFixture`, one container per run) + schema tests ✅
- 3.4 `AuditInterceptor` (CreatedAt/UpdatedAt/CreatedBy via `ICurrentUser` + `TimeProvider`) ✅

Test suite: 54 unit + 10 integration, all green.

**Next — step 4: authentication.** `POST /users` (register, `PasswordHasher<User>`), `POST /auth/login`
(JWT with `ClaimTypes.NameIdentifier` = user id, so `HttpContextCurrentUser` resolves), `GET/PUT /users/me`.
Design settled: ADR 0004 (data access + use-case shape), ADR 0005 (auth + HTTP test isolation with
Respawn). Integration tests via `WebApplicationFactory` pointing at the Testcontainers DB.

Then step 5: Projects end to end; step 6: GitHub Actions CI (build, test, coverage, SQL image as config).

Open items noted for later: .NET 9 → 10 (LTS) upgrade; `/health/live` vs `/health/ready` when
containerizing; least-privilege SQL login for the app; Identity migration as a separate exercise;
Result pattern if Application fills with try/catch; Stryker mutation run over Domain.

## How AI assistance is used

The developer writes setup and first-time code with AI guidance and review; tests are written
jointly, with the AI taking more of the boilerplate over time. Design decisions are discussed and
recorded in ADRs; the AI is listed as "Consulted" where it took part. Explanations over generated code.
