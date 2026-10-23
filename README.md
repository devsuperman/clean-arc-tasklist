# clean-arc
A backend for a task list to study Clean Architecture.

> **This is an AI Native project.** It was planned and is being built together with AI (Claude Code), as a hands-on way to study Clean Architecture in .NET 10.

## Planning docs

| Doc | Content |
|---|---|
| [Learn: Clean Architecture](docs/00-learn-clean-architecture.md) | Study guide: what Clean Architecture is, the problem it solves, and how this project applies it |
| [Business rules](docs/01-business-rules.md) | Entities, rules (with IDs), use cases, error catalog, API contract |
| [Tech stack & architecture](docs/02-tech-stack.md) | .NET 10, Clean Architecture layers, patterns, tooling, CI |
| [Development tasks](docs/03-development-tasks.md) | Backlog by milestone with acceptance criteria and Definition of Done |
| [Parallel execution plan](docs/04-parallel-execution-plan.md) | Waves, lanes, critical path and merge-conflict rules for building in parallel |

## Getting started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download) and Docker.

```bash
docker compose up -d db                       # start PostgreSQL 17 on localhost:5432
dotnet tool restore                           # dotnet-ef, reportgenerator
dotnet run --project src/TaskList.Api         # API on http://localhost:5080, Scalar at /scalar
dotnet test                                   # all tests (integration tests need Docker)
```

The connection string lives in `src/TaskList.Api/appsettings.Development.json` and matches the
`db` service in `docker-compose.yml`. Override it with the `ConnectionStrings__Database` environment variable.

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
