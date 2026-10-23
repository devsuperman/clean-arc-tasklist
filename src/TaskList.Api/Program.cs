using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using TaskList.Api.Endpoints;
using TaskList.Api.Infrastructure;
using TaskList.Application;
using TaskList.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, logger) =>
{
    logger.ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    if (builder.Environment.IsDevelopment())
    {
        logger.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            formatProvider: CultureInfo.InvariantCulture);
    }
    else
    {
        logger.WriteTo.Console(new RenderedCompactJsonFormatter());
    }
});

builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Task List API";
    document.Info.Description =
        "Manage categories and tasks. Errors are RFC 9457 Problem Details with a `code` from the error catalog "
        + "(docs/01-business-rules.md) and a `traceId`.";
    return Task.CompletedTask;
}));
builder.Services.AddApiProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is missing."),
        name: "database",
        tags: ["ready"]);

var app = builder.Build();

// Outermost, so it logs the final status code. One event per request: method, path, status, elapsed, trace id.
// Bodies, headers and query strings are not logged (no sensitive data).
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateDatabaseAsync();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.MapGroup("/api/v1")
    .ProducesProblem(StatusCodes.Status500InternalServerError) // any endpoint can fail unexpectedly (Server.Unexpected)
    .MapEndpoints();

await app.RunAsync();

public partial class Program;
