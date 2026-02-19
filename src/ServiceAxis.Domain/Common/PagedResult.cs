namespace ServiceAxis.Domain.Common;

/// <summary>
/// Represents a paged result set for queries.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Gets the items for the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Gets the total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the current page number (1-based).</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the size of each page.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Gets a value indicating whether there is a previous page.</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Gets a value indicating whether there is a next page.</summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
