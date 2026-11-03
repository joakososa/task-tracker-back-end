# 0008. The User entity owns its password: hashing interface in Domain

* **State:** Accepted
* **Date:** 2026-11-03
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code
* **Amends:** ADR 0005 (where `IPasswordHasher` lives: "an `IPasswordHasher` interface in Application")

## Context
ADR 0005 placed `IPasswordHasher` in Application, so a use case would hash the password and hand the hash to `User`. While implementing registration the password policy (8–128 characters, no trimming) turned out to be a rule about the user, like the name and email limits the entity already enforces. With the hasher in Application, the entity would receive an opaque hash and could not check the policy; every use case that sets a password (registration now, a password change later) would have to remember to apply it.

Login adds the opposite question, "does this password match the stored one?", which also depends only on the user's own data.

## Decision
* **`IPasswordHasher` lives in Domain** (`Domain/Services`), with `HashPassword(password)` and `Verify(hash, password)`. It is implemented in Infrastructure with ASP.NET Core Identity's PBKDF2 `PasswordHasher<User>`, as ADR 0005 decided.
* **`User` hashes and verifies its own password.** The constructor receives the plain password and the hasher, checks the policy and stores only the hash. `User.VerifyPassword(password, hasher)` answers whether a password matches.
* **The hasher is passed to the method that needs it, not stored in the entity.** EF Core materializes entities through the private constructor and cannot inject services, and a service is not part of the user's state.
* Authentication itself (issuing tokens, failing a login) stays in Application, as ADR 0005 decided. Domain knows about passwords, not about logins.

Everything else in ADR 0005 is unchanged.

### Pros
* **The password policy is an entity rule.** No use case can create or change a password without it.
* **Domain still has no package references.** The interface is plain C#; the Identity dependency stays in Infrastructure.
* **Unit-testable** with a fake hasher, like the rest of the entity.

### Cons
* Domain depends on an abstraction of a technical service, which some DDD styles avoid.
* Callers have to pass the hasher on every call that needs it.

## Alternatives considered

* **Interface in Application, entity receives the hash** (the original ADR 0005 shape). Keeps Domain free of any service, but moves the password policy out of the entity and into each use case. Rejected.
* **Inject the hasher into the entity and keep a reference.** Fewer parameters, but entities loaded by EF would not have it, and the entity would mix state with services. Rejected.
* **A `Password` value object that hashes itself.** Elegant, but it still needs the hasher at creation and adds a type that EF must map. Can be revisited if more password rules appear.
