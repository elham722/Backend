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
    /// Skip count for pagination
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Take count for pagination
    /// </summary>
    public int Take => PageSize;
} 