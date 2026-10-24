# 0003 · PostgreSQL, tested with Testcontainers

**Status:** Accepted

## Context

We need a relational database and integration tests that we can trust. In-memory providers (EF Core InMemory,
SQLite) behave differently from the production database: no real constraints, different case-sensitivity,
different SQL translation (e.g. `CASE` for priority sorting, `NULLS LAST`, `lower()` indexes).

## Decision

- PostgreSQL 17 via Npgsql + EF Core, with snake_case names and enums stored as strings.
- Rules that must hold even under concurrency are also enforced by the database: unique index on
  `lower(name)` (BR-C01) and a `RESTRICT` foreign key (BR-C04).
- Integration tests run the real API (`WebApplicationFactory`) against a real PostgreSQL started by
  **Testcontainers**: one container per test assembly, schema migrated once, **Respawn** empties the data
  before each test.

## Consequences

- Tests exercise the same SQL as production; projections and filters are proven on PostgreSQL.
- Running integration tests needs Docker (available locally and on GitHub-hosted runners).
- Integration tests run one at a time because they share the database; the suite stays fast because the
  container and schema are created only once.
