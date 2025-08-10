namespace Backend.Application.Common.DTOs;

/// <summary>
/// DTO for pagination parameters
/// </summary>
public class PaginationDto
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    /// <summary>
    /// Search term for filtering
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; }

    /// <summary>
    /// Additional filters as key-value pairs
    /// </summary>
    public Dictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// Include related entities
    /// </summary>
    public string? Include { get; set; }

    /// <summary>
    /// Select specific fields
    /// </summary>
    public string? Select { get; set; }

    /// <summary>
    /// Skip count for pagination
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Take count for pagination
    /// </summary>
    public int Take => PageSize;

    /// <summary>
    /// Gets filter value by key
    /// </summary>
    /// <param name="key">Filter key</param>
    /// <returns>Filter value</returns>
    public string? GetFilter(string key)
    {
        return Filters?.GetValueOrDefault(key);
    }

    /// <summary>
    /// Sets filter value
    /// </summary>
    /// <param name="key">Filter key</param>
    /// <param name="value">Filter value</param>
    public void SetFilter(string key, string value)
    {
        Filters ??= new Dictionary<string, string>();
        Filters[key] = value;
    }

    /// <summary>
    /// Checks if filter exists
    /// </summary>
    /// <param name="key">Filter key</param>
    /// <returns>True if filter exists</returns>
    public bool HasFilter(string key)
    {
        return Filters?.ContainsKey(key) == true;
    }

    /// <summary>
    /// Gets included entities as array
    /// </summary>
    /// <returns>Array of included entities</returns>
    public string[] GetIncludes()
    {
        if (string.IsNullOrWhiteSpace(Include))
            return Array.Empty<string>();

        return Include.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
    }

    /// <summary>
    /// Gets selected fields as array
    /// </summary>
    /// <returns>Array of selected fields</returns>
    public string[] GetSelects()
    {
        if (string.IsNullOrWhiteSpace(Select))
            return Array.Empty<string>();

        return Select.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
    }
} 