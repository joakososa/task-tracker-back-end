# 0001. Utilize a clean architecture

* **Status:** Accepted
* **Date:** 2026-09-18
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code


## Context
The project requires a foundational architectural pattern that supports sustainable growth. The key system and professional goals are:
* **High testability:** The ability to test business logic in isolation, without relying on databases or external web services.
* **Robust automation:** Seamless integration with CI/CD pipelines and smooth deployment to AWS environments (e.g., Lambda, ECS, or EC2).
* **Industry relevance:** Implementing an architecture widely utilized in real-world production scenarios to maximize learning value.

## Decision
We will adopt **Clean Architecture** as the structural pattern for the project. 

### Pros
* **Framework Isolation:** Business rules do not depend on EF Core or ASP.NET, which keeps them unit-testable and stable across framework upgrades.
* **Testability:** Business rules can be heavily unit-tested by leveraging dependency injection and mocking infrastructure layers.
* **Team Scalability:** Provides a clear, predictable structure, making it obvious where new features and code should live.

### Cons
* **Initial Complexity:** Requires creating multiple files, interfaces, and data mappers from day one, which may slightly slow down the initial development velocity.
* **Learning Curve:** Demands strict discipline to avoid violating the dependency rule.

## Alternatives considered

* **Vertical slices (single project, folder per feature).** Less ceremony and faster to start, but nothing stops a feature from reaching into EF Core or HTTP concerns directly; the dependency rule would rely on discipline instead of the compiler. Also less representative of what the author expects to find in enterprise .NET codebases.
* **Layered (Api + Core + Data).** A middle ground that merges Domain and Application into one project. Reasonable for a project of this size and used by respected teams, but it removes the compiler-enforced boundary between business rules and use-case orchestration, which is one of the things this project intends to practice.