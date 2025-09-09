using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Areas.Admin.ViewModels
{
    /// <summary>
    /// ViewModel for Create User page
    /// </summary>
    public class CreateUserViewModel
    {
        /// <summary>
        /// User creation data
        /// </summary>
        public CreateUserDto UserData { get; set; } = new();

        /// <summary>
        /// Available roles for selection
        /// </summary>
        public List<string> AvailableRoles { get; set; } = new();

        /// <summary>
        /// Selected roles
        /// </summary>
        public List<string> SelectedRoles { get; set; } = new();

        /// <summary>
        /// Password confirmation (not part of DTO)
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}