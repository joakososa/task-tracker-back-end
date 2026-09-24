# 0005. JWT authentication with own users, and Respawn for HTTP test isolation

* **State:** Accepted
* **Date:** 2026-09-24
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code

## Context
Step 4 adds registration (`POST /users`), login (`POST /auth/login`) and the current user's profile (`GET/PUT /users/me`). The project already has its own `User` entity (not ASP.NET Identity), an `ICurrentUser` abstraction resolved from `HttpContext` (`HttpContextCurrentUser`), and an `AuditInterceptor` that fills `CreatedBy` from it.

These endpoints are also the first HTTP integration tests. They run through `WebApplicationFactory` against the shared Testcontainers SQL Server (`SqlServerFixture`, one container per run), so tests need a way to start from a known database state without restarting the container.

## Decision

### Authentication
* **Stateless JWT bearer tokens** issued by `POST /auth/login` and validated with `Microsoft.AspNetCore.Authentication.JwtBearer`.
* The token carries **`ClaimTypes.NameIdentifier` = user id**, so `HttpContextCurrentUser` and the `AuditInterceptor` work unchanged.
* Signing key, issuer, audience and lifetime come from configuration: `dotnet user-secrets` locally, environment or secret store when deployed. **Never committed.**
* **Password hashing via an `IPasswordHasher` interface in Application**, implemented in Infrastructure with ASP.NET Core's `PasswordHasher<User>` (PBKDF2 with a built-in format version for future rehashing). Only the hasher is used, not the rest of Identity.
* Token generation sits behind an `ITokenService` interface in Application, implemented in Infrastructure, so login is a regular handler (ADR 0004).
* Failed login returns the same error for an unknown email and a wrong password, so accounts cannot be enumerated.

### HTTP test isolation
* **Respawn** resets the database **before each HTTP integration test**: it deletes rows from all tables in FK-safe order, while keeping the migrations history and seeded lookup data (`TaskStates`).
* **The tables to keep are derived from the EF model**, not listed by hand. Lookup tables are persistence classes in Infrastructure (e.g. `TaskStateLookup`; the Domain only knows the enum), and each implements an empty marker interface `ILookupTable`, also in Infrastructure. The test setup adds every entity type implementing it to Respawn's `TablesToIgnore`, plus `__EFMigrationsHistory`. A new lookup table only needs the marker, which sits next to its seed.
* The `WebApplicationFactory` points at the Testcontainers connection string. Tests authenticate by registering and logging in through the API, or with a test helper that issues a valid token.
* **HTTP test classes run sequentially for now.** If the suite becomes slow, the planned escape hatch is **one database per xUnit collection** inside the same container (each created and migrated on start, each reset by its own Respawn checkpoint). Collections then run in parallel, and tests inside a collection stay sequential. The container, the most expensive part, is still shared.

### Pros
* **No session state on the server**: fits horizontal scaling, containers and Azure Functions (steps 5 and 12).
* **Existing auditing works as is**: the claim maps directly to `ICurrentUser`.
* **Secrets out of the repository** from day one.
* **Tests exercise the real pipeline**: each request runs in its own DI scope and `DbContext` with real commits, exactly as in production.
* **Fast reset**: deleting rows is much cheaper than recreating the schema or the container.

### Cons
* **JWTs cannot be revoked before expiry** without extra infrastructure (deny list, short lifetime + refresh tokens). Accepted for now with a short lifetime. Refresh tokens are out of scope.
* Own user management means owning its security details (hashing, enumeration, lockout). Lockout and email confirmation are out of scope for this learning step.
* Tests that share the database cannot run in parallel against it. Acceptable while HTTP tests take tens of milliseconds each, with the per-collection database as the documented way out.
* One more dependency (Respawn) and a reset call to remember in the test base class.

## Alternatives considered

* **ASP.NET Core Identity.** Complete (lockout, confirmation, 2FA) but brings its own schema and `IdentityUser`, which would replace the domain `User` and hide the mechanics this project wants to practise. Deferred as a separate migration exercise.
* **External identity provider (Microsoft Entra External ID, Auth0, Cognito).** Production-grade and likely relevant once deployed to Azure, but it hands off exactly the mechanics this project sets out to relearn. Can be revisited in step 5.
* **Cookie-based sessions.** Natural for server-rendered apps. Less so for an API consumed by an SPA or other services. Rejected.
* **Transaction per test, rolled back at the end.** Fast, but the HTTP request runs in a different DI scope with its own `DbContext` and connection, so it does not see, or join, the test's transaction. Rejected.
* **Recreate the database or container per test.** Fully isolated but far too slow. Rejected.
* **Unique data per test without cleanup.** Allows parallelism, but assertions on lists (e.g. "my projects") become fragile and the database grows across the run. Rejected as the default.
