using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskList.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> (migrations, bundles) so they do not need to start the API host.
/// The connection string comes from <c>ConnectionStrings__Database</c>; migration bundles override it with
/// <c>--connection</c>.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string LocalDevelopment =
        "Host=localhost;Port=5432;Database=tasklist;Username=tasklist;Password=tasklist";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database") ?? LocalDevelopment;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
