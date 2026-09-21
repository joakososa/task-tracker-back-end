# 0003. Model task state transitions as an explicit transition table

* **State:** Accepted
* **Date:** 2026-09-20
* **Decision-maker:** Developer Joaquin Sosa
* **Consulted:** Claude Code

## Context
A task has a state (`Todo`, `InProgress`, `Done`) and RF-12 allows members to change it. The functional specification (RF-10) leaves the allowed transitions open and points to this ADR. The rule adopted for the product is that **tasks only move forward**: `Todo → InProgress → Done`, skipping is allowed (`Todo → Done`), moving backwards or to the same state is not.

The rule has to live in the Domain layer so that it is unit-testable and cannot be bypassed by use cases, and it must remain correct if new states are added later (e.g. `Blocked`).

## Decision
`TaskItem.ChangeState(newState)` validates the transition against a static, explicit transition table:

```csharp
[TaskState.Todo]       = [TaskState.InProgress, TaskState.Done],
[TaskState.InProgress] = [TaskState.Done],
[TaskState.Done]       = [],
```

An invalid transition throws a `DomainException` with code `TASK_INVALID_STATE_TRANSITION` and leaves the entity unchanged. All nine `from → to` combinations are covered by unit tests.

### Pros
* **The rule is readable as a rule.** The table is the state machine; there is nothing to infer.
* **Safe under change.** Adding a state without touching the table fails loudly (`KeyNotFoundException` on first use, and the unit tests break), so it cannot be forgotten.
* **No hidden coupling to enum values.** The numeric values of `TaskState` are used only for persistence (see the `TaskStates` lookup table), never for business logic.

### Cons
* Slightly more code than a comparison, and the table must be kept in sync with the enum by hand (mitigated by the tests).

## Alternatives considered

* **Numeric comparison (`newState > State`).** Shorter, but it silently depends on the enum being declared in order. A developer inserting a new state would break the rule without any signal pointing here. Rejected for that reason.
* **Allow any transition (Trello-like).** Simplest, but it leaves no business rule to model or test in the Domain, and it does not match the intended product behaviour.
* **A full state-machine library.** Overkill for three states and one rule.
