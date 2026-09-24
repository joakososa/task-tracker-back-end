# 0004. Repositories per aggregate and one handler per use case

* **State:** Accepted
* **Date:** 2026-09-24
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code

## Context
Step 4 (authentication) introduces the first use cases, so the Application layer needs a shape that every later feature (Projects, Tasks, RF-01..RF-24) will follow. Three questions have to be answered together:

1. **How does Application reach the data?** Application must not depend on EF Core (ADR 0001). Business rules that span entities (e.g. "the assignee must be a project member") live in Application and should be unit-testable without a database. The roadmap also moves parts of the model to Cosmos DB and MongoDB later (steps 10–11), so persistence has to be swappable per aggregate.
2. **What does a use case look like?** Use cases should be small, independently testable, and callable from entry points other than HTTP (an Azure Function in step 12, a message consumer in step 8).
3. **Where does validation live, and how do errors reach the client?** Domain entities already enforce invariants with `Guard` and `DomainException`, but they stop at the first error and know nothing about HTTP.

## Decision

### Data access
* **One repository per aggregate root**, e.g. `IUserRepository`, `IProjectRepository`. Child entities (e.g. `ProjectMember`) are reached through their root and get no repository of their own.
* Repository methods are **added only when a use case needs them** (`ExistsByEmailAsync`, `GetByIdAsync`…). There is no generic `IRepository<T>` and no speculative CRUD.
* **`IUnitOfWork.SaveChangesAsync()`** commits changes. The handler decides when to commit, and `AppDbContext` implements it.
* Interfaces live in Application and EF implementations in Infrastructure. Repositories return **domain entities**: EF maps them directly through the existing `IEntityTypeConfiguration<T>` classes, with no separate persistence models.

### Use cases
* **One handler per use case** (`RegisterUserHandler.HandleAsync(RegisterUserCommand, CancellationToken)`), injected directly into the endpoint. **No MediatR.**
* **Light CQRS:** inputs are immutable records named `…Command` (changes state) or `…Query` (reads only). Commands go through the domain. Queries may project straight to DTOs. Reads and writes share the same database.
* The endpoint maps the HTTP request DTO (from `docs/openapi.yaml`) to the command. The command is the use case's contract, and the DTO is the HTTP contract.

### Validation and errors
* **FluentValidation validators on commands, in Application**, so every entry point is validated, not only HTTP. Validators cover rules the client should see per field and all at once (email format, password length). They do not duplicate every `Guard` by default.
* **Domain `Guard`s remain the last line of defence**: an entity can never be built in an invalid state.
* **One `IExceptionHandler` in Api** maps validation failures to `400` with per-field errors, and each `DomainException` code to its HTTP status (e.g. `TASK_INVALID_STATE_TRANSITION` → `409`). Responses use the `Error` schema from `docs/openapi.yaml`. Domain and Application never know HTTP.

### Pros
* **Application has no EF dependency**, which keeps the Clean Architecture claim honest.
* **Cross-entity rules are unit-testable** with in-memory fake repositories. Testcontainers is reserved for repository implementations and HTTP flows.
* **Persistence can change per aggregate** (e.g. to Cosmos DB) by adding an implementation in Infrastructure, without touching handlers.
* **Handlers map cleanly to other entry points**: an Azure Function or a message consumer builds the same command and calls the same handler.
* **No magic and no licensing concerns**: dependency flow is visible in constructors.

### Cons
* More types than injecting `DbContext` directly: an interface and an implementation per aggregate, plus a command, handler and validator per use case.
* Repository methods can drift into thin wrappers around LINQ. Mitigated by adding them only on demand.
* Without MediatR, cross-cutting behaviour (logging, transactions) around all handlers needs decorators if it is ever required.
* Some validation rules exist twice (validator and `Guard`) when both the client and the invariant need them.

## Alternatives considered

* **`IAppDbContext` exposing `DbSet<T>` in Application.** Popular (e.g. Jason Taylor's Clean Architecture template) and less code, but it puts EF in Application's public surface, contradicting ADR 0001. It forces database-backed tests for cross-entity rules and ties every handler to EF when an aggregate moves to Cosmos DB or MongoDB. Rejected.
* **Generic `IRepository<T>`.** Encourages a CRUD surface that no use case asked for and leaks query concerns (`IQueryable`) back into Application. Rejected.
* **Separate persistence models mapped to domain entities.** Maximum isolation, but doubles the model and adds mapping code with no current benefit, since EF configurations already keep persistence details out of the entities. Rejected.
* **Service classes (`UserService` with many methods).** They tend to grow without bound, mix unrelated dependencies, and cannot be deployed or tested per use case. Rejected.
* **MediatR.** Adds indirection (`mediator.Send`) and pipeline behaviours that a small monolith does not need. Since 2025 new versions are commercially licensed. Rejected. It can be revisited if cross-cutting pipelines become necessary.
* **Validation in Api (on request DTOs).** Simpler with ASP.NET integration, but any non-HTTP entry point would bypass it. Api keeps only what is inherently HTTP (malformed JSON, type binding). Rejected.
* **Full CQRS / event sourcing.** Separate read stores or an event log are unjustified for this scope. Rejected.
