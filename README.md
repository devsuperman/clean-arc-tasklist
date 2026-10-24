# clean-arc-tasklist

A backend for a task list, built to study **Clean Architecture** in **.NET 10**.

> **This is an AI Native project.** It was planned and built together with AI (Claude Code), as a hands-on way
> to study Clean Architecture. The backlog was executed in parallel lanes — see the
> [parallel execution plan](docs/04-parallel-execution-plan.md).

New to Clean Architecture? Start with the study guide: [Learn: Clean Architecture](docs/00-learn-clean-architecture.md).

## What it does

A single-user REST API (`/api/v1`) that manages **categories** and **tasks**:

- Category CRUD with case-insensitive unique names; a category with tasks cannot be deleted.
- Task CRUD with a status lifecycle (`Pending → InProgress → Done`, reopen), read-only completed tasks and
  due dates that cannot be in the past.
- Task listing with filters (category, status, priority, due date range, overdue, text search), sorting and paging.
- Errors are [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457) with a `code` from the
  [error catalog](docs/01-business-rules.md#6-error-catalog) and a `traceId` that can be found in the logs.

| Method | Route | Use case |
|---|---|---|
| POST | `/categories` | Create category |
| GET | `/categories/{id}` | Get category (with task counters) |
| GET | `/categories?page&pageSize&search` | List categories |
| PUT | `/categories/{id}` | Update category |
| DELETE | `/categories/{id}` | Delete category |
| GET | `/categories/{id}/tasks?...` | List the tasks of a category |
| POST | `/tasks` | Create task |
| GET | `/tasks/{id}` | Get task |
| GET | `/tasks?categoryId&status&priority&dueFrom&dueTo&overdue&search&sortBy&sortDirection&page&pageSize` | List tasks |
| PUT | `/tasks/{id}` | Update task |
| POST | `/tasks/{id}/start` · `/complete` · `/reopen` | Change status |
| DELETE | `/tasks/{id}` | Delete task |

The full rules are in [docs/01-business-rules.md](docs/01-business-rules.md).

## Getting started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker](https://docs.docker.com/get-docker/).

```bash
git clone https://github.com/devsuperman/clean-arc-tasklist.git
cd clean-arc-tasklist
```

### Option A — run everything in Docker

```bash
docker compose up --build                     # PostgreSQL + API on http://localhost:8080
```

Open <http://localhost:8080/scalar> for the interactive API docs.

### Option B — run the API with the .NET SDK

```bash
docker compose up -d db                       # PostgreSQL 17 on localhost:5432
dotnet tool restore                           # dotnet-ef, reportgenerator
dotnet run --project src/TaskList.Api         # API on http://localhost:5080
```

Open <http://localhost:5080/scalar>. In Development, database migrations are applied on startup.

The connection string lives in `src/TaskList.Api/appsettings.Development.json` and matches the `db` service in
`docker-compose.yml`. Override it with the `ConnectionStrings__Database` environment variable.

### Try the API

[`src/TaskList.Api/TaskList.Api.http`](src/TaskList.Api/TaskList.Api.http) has a request for every endpoint, in an
order you can run top to bottom (Visual Studio, Rider or VS Code with the REST Client extension).

Health checks: `GET /health` (liveness) and `GET /health/ready` (includes the database).

## Running the tests

```bash
dotnet test                                   # all tests; integration tests need Docker running
```

| Project | Kind | What it covers |
|---|---|---|
| `TaskList.Domain.UnitTests` | Unit | Entities and business rules (no I/O, milliseconds) |
| `TaskList.Application.UnitTests` | Unit | Handlers, validators, projections (fakes for repositories and the clock) |
| `TaskList.Api.IntegrationTests` | Integration | Every endpoint and status code against a real PostgreSQL ([Testcontainers](https://dotnet.testcontainers.org/)), persistence, end-to-end flows |
| `TaskList.ArchitectureTests` | Architecture | The dependency rule and conventions ([NetArchTest](https://github.com/BenMorris/NetArchTest)) |

Tests run on Microsoft.Testing.Platform (configured in `global.json`). Coverage report:

```bash
dotnet test --coverlet --coverlet-output-format cobertura
dotnet reportgenerator "-reports:tests/**/TestResults/coverage.cobertura*.xml" -targetdir:coverage -reporttypes:Html
```

CI ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)) runs on every push and pull request to `main`:
format check, rule traceability, Release build with warnings as errors, all tests, and a coverage gate of
**90% line coverage for Domain + Application**.

## Project structure

```text
src/
├── TaskList.Domain/          # Entities (Category, TaskItem), rules, errors, Result — depends on nothing
├── TaskList.Application/     # Use cases: one folder per use case (command/query + handler + validator),
│                             # ports (repositories, IReadDbContext, IUnitOfWork), DTOs
├── TaskList.Infrastructure/  # EF Core + PostgreSQL: DbContext, configurations, migrations, repositories
└── TaskList.Api/             # Minimal API endpoints (one class per endpoint), Problem Details, Program.cs
tests/
├── TaskList.Domain.UnitTests/
├── TaskList.Application.UnitTests/
├── TaskList.Api.IntegrationTests/
└── TaskList.ArchitectureTests/
build/                        # CI helper scripts (coverage gate, rule traceability)
docs/                         # Study guide, business rules, architecture, backlog, ADRs
```

Dependencies point inwards only: `Api → Application → Domain`, `Infrastructure → Application`.
`Api` references `Infrastructure` only to wire dependency injection in `Program.cs`. The architecture tests
fail the build if this changes.

**Adding a use case** touches no shared file: add a folder under `src/TaskList.Application/<Feature>/` with the
command or query, its handler and validator (they are registered automatically), and one `IEndpoint` class under
`src/TaskList.Api/Endpoints/<Feature>/` (discovered automatically).

## Database migrations

- **Development:** migrations are applied automatically when the API starts.
- **Other environments:** CI publishes a self-contained migrations bundle (`efbundle-linux-x64` artifact).
  Run it in the deploy pipeline before starting the new API version:

  ```bash
  ./efbundle --connection "Host=...;Database=tasklist;Username=...;Password=..."
  ```

  Build it locally with:

  ```bash
  dotnet ef migrations bundle --project src/TaskList.Infrastructure --startup-project src/TaskList.Infrastructure \
    --self-contained -r linux-x64 -o efbundle
  ```

Create a new migration (only one schema change at a time, see the [parallel plan](docs/04-parallel-execution-plan.md#6-avoiding-merge-conflicts)):

```bash
dotnet ef migrations add <Name> --project src/TaskList.Infrastructure --output-dir Persistence/Migrations
```

## Documentation

| Doc | Content |
|---|---|
| [Learn: Clean Architecture](docs/00-learn-clean-architecture.md) | Study guide: what Clean Architecture is, the problem it solves, and how this project applies it |
| [Business rules](docs/01-business-rules.md) | Entities, rules (with IDs), use cases, error catalog, API contract |
| [Tech stack & architecture](docs/02-tech-stack.md) | .NET 10, Clean Architecture layers, patterns, tooling, CI |
| [Development tasks](docs/03-development-tasks.md) | Backlog by milestone with acceptance criteria and Definition of Done |
| [Parallel execution plan](docs/04-parallel-execution-plan.md) | Waves, lanes, critical path and merge-conflict rules for building in parallel |
| [Rule traceability](docs/05-rule-traceability.md) | Each business rule ID and the tests that cover it |
| [Architecture decision records](docs/adr/README.md) | No MediatR, Result pattern, PostgreSQL + Testcontainers, read side without EF Core |
