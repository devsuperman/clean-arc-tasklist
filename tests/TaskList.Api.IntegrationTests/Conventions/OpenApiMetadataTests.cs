using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TaskList.Api.IntegrationTests.Infrastructure;

namespace TaskList.Api.IntegrationTests.Conventions;

/// <summary>TL-038: every API endpoint is fully documented in OpenAPI (Scalar).</summary>
public sealed class OpenApiMetadataTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public void EveryApiEndpoint_HasNameTagSummaryAndDocumentedResponses()
    {
        var endpoints = Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith("/api/v1", StringComparison.Ordinal) == true)
            .ToList();

        endpoints.Count.ShouldBe(14); // the API contract in docs/01-business-rules.md §7

        foreach (var endpoint in endpoints)
        {
            var route = endpoint.RoutePattern.RawText;
            var statusCodes = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .Select(metadata => metadata.StatusCode)
                .ToList();

            endpoint.Metadata.GetMetadata<IEndpointNameMetadata>().ShouldNotBeNull($"{route} has no name");
            endpoint.Metadata.GetMetadata<ITagsMetadata>().ShouldNotBeNull($"{route} has no tag");
            endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>().ShouldNotBeNull($"{route} has no summary");
            statusCodes.ShouldContain(code => code >= 200 && code < 300, $"{route} documents no success response");
            statusCodes.ShouldContain(code => code >= 400 && code < 500, $"{route} documents no client error");
            statusCodes.ShouldContain(500, $"{route} does not document 500");
        }
    }
}
