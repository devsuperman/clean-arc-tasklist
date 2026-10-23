# 0001 · Hand-written handlers instead of MediatR

**Status:** Accepted

## Context

Each use case is a command or query with one handler. MediatR is the usual library for dispatching them and
for cross-cutting "pipeline behaviors" (validation, logging). Its recent major versions moved to a commercial
license, and this is a study project that keeps only permissive (MIT/Apache) dependencies. A mediator also
hides the call graph: from an endpoint you cannot "go to definition" of the code that runs.

## Decision

- Define our own tiny interfaces: `ICommand`, `ICommand<T>`, `IQuery<T>`, `ICommandHandler<…>`, `IQueryHandler<…>`.
- Endpoints receive the handler interface directly by dependency injection and call `Handle(...)`.
- `AddApplication()` registers every handler by assembly scanning and wraps it in a **validation decorator**
  (`ValidationDecorator`), which plays the role of a MediatR pipeline behavior.

## Consequences

- No third-party dispatcher; ~60 lines of our own code that are easy to read and debug.
- The dependency from an endpoint to its use case is explicit in the endpoint's signature.
- New cross-cutting concerns need a new decorator registered in `AddApplication()` (one place).
- New use cases need no registration code.
