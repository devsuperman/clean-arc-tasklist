# Architecture Decision Records

Short records of the decisions that shape this project: the context, the decision and its consequences.

| # | Decision | Status |
|---|---|---|
| [0001](0001-no-mediatr.md) | Hand-written handlers instead of MediatR | Accepted |
| [0002](0002-result-pattern.md) | Result pattern instead of exceptions for expected failures | Accepted |
| [0003](0003-postgresql-and-testcontainers.md) | PostgreSQL, tested with Testcontainers | Accepted |
| [0004](0004-read-side-without-ef-core.md) | Query handlers use `IReadDbContext` (plain LINQ), not EF Core | Accepted |
