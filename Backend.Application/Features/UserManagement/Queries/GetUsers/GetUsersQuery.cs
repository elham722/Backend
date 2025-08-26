using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Common.DTOs;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Queries.GetUsers;

/// <summary>
/// Query to get a paginated list of users
/// </summary>
public class GetUsersQuery : IQuery<Result<PaginationResponse<UserDto>>>
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Search term for email, username, or phone number
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Filter by user status
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Filter by role
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Filter by email confirmation status
    /// </summary>
    public bool? EmailConfirmed { get; set; }
    
    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Sort by field
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Whether to include deleted users
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;
} 