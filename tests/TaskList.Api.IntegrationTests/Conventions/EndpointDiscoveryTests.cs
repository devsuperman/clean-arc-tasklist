using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskList.Api.Endpoints;

namespace TaskList.Api.IntegrationTests.Conventions;

public sealed class EndpointDiscoveryTests
{
    public sealed class SampleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app) => app.MapGet("/sample", () => Results.Ok());
    }

    [Fact]
    public async Task MapEndpoints_DiscoversEveryIEndpoint()
    {
        await using var app = WebApplication.CreateBuilder().Build();

        app.MapGroup("/api/v1").MapEndpoints(typeof(SampleEndpoint).Assembly);

        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText);
        routes.ShouldContain("/api/v1/sample");
    }
}
