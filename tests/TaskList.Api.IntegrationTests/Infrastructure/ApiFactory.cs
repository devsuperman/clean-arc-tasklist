using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using TaskList.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace TaskList.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Runs the real API against a real PostgreSQL (Testcontainers). One instance is shared by the whole test
/// assembly; <see cref="ResetAsync"/> empties the database and resets the clock before each test.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private Respawner _respawner = null!;

    public TestTimeProvider Clock { get; } = new();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        await using (var scope = Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }

        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"],
        });
    }

    public async Task ResetAsync()
    {
        Clock.UtcNow = TestTimeProvider.DefaultNow;

        await using var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _database.GetConnectionString());
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
