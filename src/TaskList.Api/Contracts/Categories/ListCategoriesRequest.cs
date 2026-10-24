namespace TaskList.Api.Contracts.Categories;

/// <summary>Query string of <c>GET /categories</c> (BR-L01, BR-L06).</summary>
public sealed record ListCategoriesRequest(int Page = 1, int PageSize = 20, string? Search = null);
