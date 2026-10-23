namespace TaskList.Application.Common.Paging;

/// <summary>A query that returns a page of results (BR-L01).</summary>
public interface IPagedQuery
{
    int Page { get; }

    int PageSize { get; }
}
