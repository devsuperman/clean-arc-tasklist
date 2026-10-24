# 0004 · Query handlers use `IReadDbContext` (plain LINQ), not EF Core

**Status:** Accepted

## Context

Queries should read efficiently: no change tracking, project straight to response DTOs, filter, sort and
page in SQL. The simple way is to use EF Core's `DbContext` in the Application layer, but that breaks the
dependency rule (Application would depend on a framework). Repository methods per query would avoid that,
but every query use case would edit the same repository files, which blocks
[parallel work](../04-parallel-execution-plan.md).

## Decision

- Application defines `IReadDbContext`: `IQueryable<Category>`, `IQueryable<TaskItem>` and three async
  executors (`ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`).
- Query handlers compose queries with plain `System.Linq.Queryable` operators and shared projections
  (`SelectResponse`); Infrastructure executes them with EF Core and `AsNoTracking()`.
- Unit tests back `IReadDbContext` with in-memory lists; integration tests prove the SQL translation.

## Consequences

- Application has no EF Core reference (checked by the architecture tests).
- Each query use case lives in its own folder and touches no shared file.
- Some LINQ that works in memory might not translate to SQL; integration tests are the source of truth for
  query behavior.
