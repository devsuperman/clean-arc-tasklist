# 0002 · Result pattern instead of exceptions for expected failures

**Status:** Accepted

## Context

Many failures are part of the business rules: a task is not found, a status transition is not allowed, a
category name already exists. Throwing exceptions for them hides control flow, makes them easy to forget to
handle, and pushes HTTP knowledge (status codes) toward the inner layers.

## Decision

- Domain methods and handlers return `Result` / `Result<T>`. A failure carries an `Error(Code, Message, Type)`.
- Errors are named after the [error catalog](../01-business-rules.md#6-error-catalog) (`TaskErrors.InvalidStatusTransition`, …).
  Field validation failures are a `ValidationError` with the messages per field.
- `ErrorType` (`Validation`, `NotFound`, `Conflict`, `Unexpected`) is the only "severity" the inner layers know.
  The Api maps it to HTTP once, in `ResultExtensions.ToProblem()` (400/404/409/500 Problem Details).
- Exceptions are reserved for bugs and infrastructure failures; the global exception handler turns them into
  `500 Server.Unexpected` without leaking details.

## Consequences

- Every expected failure is visible in method signatures and is covered by tests.
- The Domain and Application layers stay free of HTTP.
- A bit more ceremony (`if (result.IsFailure) return result;`) than throwing.
