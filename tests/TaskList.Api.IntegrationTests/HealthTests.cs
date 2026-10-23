using System.Net;
using TaskList.Api.IntegrationTests.Infrastructure;

namespace TaskList.Api.IntegrationTests;

public sealed class HealthTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Ready_WithDatabaseUp_ReturnsHealthy()
    {
        var response = await Client.GetAsync("/health/ready", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(CancellationToken)).ShouldBe("Healthy");
    }

    [Fact]
    public async Task Live_ReturnsHealthy() =>
        (await Client.GetAsync("/health", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.OK);

    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetailsWithTraceId()
    {
        var response = await Client.GetAsync("/api/v1/does-not-exist", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadProblemAsync(response)).TraceId.ShouldNotBeNullOrEmpty();
    }
}
