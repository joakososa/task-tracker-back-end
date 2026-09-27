# 0007. Uniform error responses and validation layers

* **State:** Accepted
* **Date:** 2026-09-27
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code
* **Amends:** ADR 0004 (its validation criterion: "validators do not duplicate every `Guard` by default")

## Context
A request can fail at three different points, and each one produces a different kind of failure:

1. **Model binding** (Api): the body is malformed JSON, a field has the wrong type, or the body is missing. `[ApiController]` rejects the request with an automatic `400` before any of our code runs, using ASP.NET Core's **ProblemDetails** format (RFC 9457).
2. **Input validation** (Application): FluentValidation evaluates every rule of the command and throws a single `ValidationException` carrying all failures.
3. **Domain rules** (Domain/Application): an entity `Guard`, a behaviour method or a handler throws a `DomainException` with a code, and stops at the first problem.

The contract (`docs/openapi.yaml`) defines one `Error` schema (`code`, `message`, optional `errors` per field) for every error response. Left alone, the API would answer with two different shapes depending on where the request failed.

The HTTP status for a domain error must be decided somewhere without teaching the Domain about HTTP, and without a per-code table in Api that can silently fall out of date when a new code is added.

ADR 0004 also said validators should not repeat rules already enforced by `Guard`s. In practice this produced inconsistent responses: some fields were reported all at once (validator), others one at a time (`Guard`).

## Decision

### One response shape
Every error response uses the contract's `Error` schema, serialized from `ErrorResponse(Code, Message, Errors?)`. Null `errors` are omitted.

| Failure | Status | `code` | `errors` |
|---|---|---|---|
| Model binding | 400 | `INVALID_INPUT` | per field (`$.email` → `email`; missing body → `body`) |
| `ValidationException` | 400 | `INVALID_INPUT` | per field, camelCase, all failures |
| `DomainException` | from its `ErrorKind` | the exception's code | none |
| Anything else | 500 | `INTERNAL_ERROR` | none; the message and stack trace are logged, never returned |

* `ErrorMapper` (Api) is a pure function from exception to `(status, ErrorResponse)`, unit-tested.
* `GlobalExceptionHandler : IExceptionHandler` calls it, logs only 5xx as errors, and writes the response. It is first in the pipeline.
* `InvalidModelStateResponseFactory` replaces the automatic ProblemDetails `400` with `ErrorMapper.FromModelState`.

### Error kinds instead of a code → status table
Each domain error is a `DomainError(Code, Kind)` declared in `DomainErrors`. `ErrorKind` describes the problem in business terms (`Validation`, `Conflict`, `NotFound`, `Forbidden`). Api maps the four kinds to 400, 409, 404 and 403. A new error cannot be declared without a kind, so there is nothing to forget in Api.

### Validation layers
* **The validator validates the whole command** (required fields, lengths, formats, password policy) so the client gets every error at once. Limits come from Domain constants (`User.NameMaxLength`, `User.PasswordMinLength`…), so no number is written twice.
* **The Domain re-checks its own rules** through `Guard`s and constructors, whether or not a validator ran before. An entity can never be built invalid, even from an entry point that skips validation (an Azure Function, a message consumer, a future bug).
* Rules live where the data to check them lives. An entity checks what it can see (its own fields, its state transitions). Rules that need other data (email uniqueness, "assignee is a project member") live in handlers.
* Over HTTP, a `Guard` failure should in practice never be reached. If it is, a validator rule is missing.

This replaces ADR 0004's criterion of not duplicating `Guard` rules.

### Pros
* **One error format** for clients, whatever layer failed.
* **Domain stays free of HTTP.** Kinds are business concepts, and the status mapping is four lines in Api.
* **Nothing to keep in sync per code.** The compiler enforces a kind on every error.
* **Consistent validation feedback.** All input errors come back together, and the domain still guarantees valid entities.
* **No internal details leak** on 500s.

### Cons
* Validation rules are declared twice (validator and `Guard`), although their limits are shared constants.
* A custom format instead of ProblemDetails means ASP.NET Core defaults (automatic 400s, `Results.Problem`) have to be overridden, and tooling that expects RFC 9457 will not understand our errors.
* Model-binding messages come from System.Text.Json and are in English and fairly technical.

## Alternatives considered

* **ProblemDetails (RFC 9457) everywhere.** It is the ASP.NET Core default and widely used. Rejected because the contract was defined first with `Error`, and a stable `code` string is easier for the frontend to branch on than a `type` URI. It can be revisited through the contract, since the API has no consumers yet.
* **Per-code status table in Api**, with a reflection test that fails when a code has no mapping. It works, but it adds a table and a test to maintain for every new code. Rejected in favour of `ErrorKind`.
* **HTTP status inside `DomainException`.** It is the simplest option, but it puts HTTP in the Domain, against ADR 0001. Rejected.
* **Validator only for rules the Domain does not have** (the original ADR 0004 criterion). It avoids duplicated rules, but gives inconsistent feedback. Rejected.
* **Only Domain validation, no FluentValidation.** One place for rules, but the client sees one error per round trip. Rejected.
* **Only validators, anemic entities.** Invariants could be bypassed by any entry point that skips validation. Rejected.
