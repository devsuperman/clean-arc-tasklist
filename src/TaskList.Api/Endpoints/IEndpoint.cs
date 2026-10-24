namespace TaskList.Api.Endpoints;

/// <summary>
/// One HTTP endpoint. Implementations are discovered by <see cref="EndpointExtensions.MapEndpoints(IEndpointRouteBuilder)"/>,
/// so adding an endpoint never requires changes to <c>Program.cs</c>. Convention: one class per file under
/// <c>Endpoints/&lt;Feature&gt;/</c>.
/// </summary>
public interface IEndpoint
{
    /// <param name="app">The <c>/api/v1</c> route group.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
