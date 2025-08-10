namespace Backend.Application.Common.DTOs;

/// <summary>
/// Response DTO for paginated data
/// </summary>
/// <typeparam name="T">Type of the data items</typeparam>
public class PaginationResponse<T>
{
    /// <summary>
    /// The data items
    /// </summary>
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public PaginationMetaData Meta { get; set; } = new();

    /// <summary>
    /// Links for navigation
    /// </summary>
    public PaginationLinks Links { get; set; } = new();

    public PaginationResponse()
    {
    }

    public PaginationResponse(IEnumerable<T> data, PaginationMetaData meta, PaginationLinks links)
    {
        Data = data;
        Meta = meta;
        Links = links;
    }

    /// <summary>
    /// Creates a pagination response from paginated result
    /// </summary>
    /// <param name="result">Paginated result</param>
    /// <param name="baseUrl">Base URL for links</param>
    /// <returns>Pagination response</returns>
    public static PaginationResponse<T> FromResult(Common.Results.PaginatedResult<T> result, string baseUrl = "")
    {
        if (!result.IsSuccess || result.Data == null)
        {
            return new PaginationResponse<T>
            {
                Data = Enumerable.Empty<T>(),
                Meta = new PaginationMetaData(),
                Links = new PaginationLinks()
            };
        }

        var meta = new PaginationMetaData
        {
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            HasPreviousPage = result.HasPreviousPage,
            HasNextPage = result.HasNextPage
        };

        var links = new PaginationLinks
        {
            First = $"{baseUrl}?pageNumber=1&pageSize={result.PageSize}",
            Last = $"{baseUrl}?pageNumber={result.TotalPages}&pageSize={result.PageSize}",
            Previous = result.HasPreviousPage ? $"{baseUrl}?pageNumber={result.PageNumber - 1}&pageSize={result.PageSize}" : null,
            Next = result.HasNextPage ? $"{baseUrl}?pageNumber={result.PageNumber + 1}&pageSize={result.PageSize}" : null
        };

        return new PaginationResponse<T>(result.Data, meta, links);
    }
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMetaData
{
    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Start index of current page
    /// </summary>
    public int StartIndex => (PageNumber - 1) * PageSize + 1;

    /// <summary>
    /// End index of current page
    /// </summary>
    public int EndIndex => Math.Min(PageNumber * PageSize, TotalCount);
}

/// <summary>
/// Pagination links
/// </summary>
public class PaginationLinks
{
    /// <summary>
    /// Link to first page
    /// </summary>
    public string? First { get; set; }

    /// <summary>
    /// Link to last page
    /// </summary>
    public string? Last { get; set; }

    /// <summary>
    /// Link to previous page
    /// </summary>
    public string? Previous { get; set; }

    /// <summary>
    /// Link to next page
    /// </summary>
    public string? Next { get; set; }
} 