using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;

namespace Client.MVC.Areas.Admin.ViewModels
{
    /// <summary>
    /// ViewModel for Users Index page
    /// </summary>
    public class UsersIndexViewModel
    {
        /// <summary>
        /// Paginated list of users
        /// </summary>
        public PaginatedResult<UserDto> Users { get; set; } = PaginatedResult<UserDto>.Failure("خطا در دریافت کاربران");

        /// <summary>
        /// Current search term
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Available roles for filtering
        /// </summary>
        public List<string> AvailableRoles { get; set; } = new();

        /// <summary>
        /// Whether current user can delete users
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Current admin user ID
        /// </summary>
        public string CurrentAdminId { get; set; } = string.Empty;
    }
}