using Backend.Application.Common.Results;
using Backend.Application.Common.Authorization;
using Client.MVC.Services.Admin;
using Backend.Application.Features.UserManagement.DTOs;
using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for user management in admin panel
    /// Admin and SuperAdmin can manage users
    /// </summary>
    [Area("Admin")]
    [Authorize(Policy = RoleBasedAuthorizationPolicies.CanManageUsers)]
    public class UsersController : AdminBaseController
    {
        private readonly IAdminUserService _adminUserService;

        public UsersController(
            IAdminUserService adminUserService,
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger<UsersController> logger)
            : base(currentUser, antiForgeryService, logger)
        {
            _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
        }

        /// <summary>
        /// Displays the list of users
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                SetAdminViewData();

                // Get users with pagination
                var usersResult = await _adminUserService.GetUsersAsync(page, pageSize, searchTerm);
                var users = usersResult.IsSuccess ? usersResult : PaginatedResult<UserDto>.Failure("خطا در دریافت کاربران");

                // Get available roles
                var rolesResult = await _adminUserService.GetAvailableRolesAsync();
                var availableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<string>();

                // Create view model using anonymous object
                var viewModel = new
                {
                    Users = users,
                    SearchTerm = searchTerm,
                    CurrentPage = page,
                    PageSize = pageSize,
                    AvailableRoles = availableRoles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while loading users list");
                return AdminError("خطا در بارگذاری لیست کاربران", ex.Message);
            }
        }

        /// <summary>
        /// Displays user details
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                SetAdminViewData();

                // Get user details
                var userResult = await _adminUserService.GetUserByIdAsync(id);
                if (!userResult.IsSuccess)
                {
                    return NotFound();
                }

                // Get available roles
                var rolesResult = await _adminUserService.GetAvailableRolesAsync();
                var availableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<string>();

                var viewModel = new
                {
                    User = userResult.Data!,
                    AvailableRoles = availableRoles,
                    UserRoles = userResult.Data!.Roles ?? new List<string>(),
                    CanEdit = IsSuperAdmin || userResult.Data!.Id != CurrentAdminId,
                    CanDelete = IsSuperAdmin && userResult.Data!.Id != CurrentAdminId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while loading user details for ID: {UserId}", id);
                return AdminError("خطا در بارگذاری جزئیات کاربر", ex.Message);
            }
        }

        /// <summary>
        /// Displays create user form
        /// </summary>
        public async Task<IActionResult> Create()
        {
            try
            {
                SetAdminViewData();

                // Get available roles
                var rolesResult = await _adminUserService.GetAvailableRolesAsync();
                var availableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<string>();

                var viewModel = new
                {
                    AvailableRoles = availableRoles
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while loading create user form");
                return AdminError("خطا در بارگذاری فرم ایجاد کاربر", ex.Message);
            }
        }

        /// <summary>
        /// Handles create user form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Get available roles again for the form
                    var rolesResult = await _adminUserService.GetAvailableRolesAsync();
                    var availableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<string>();
                    
                    var viewModel = new
                    {
                        AvailableRoles = availableRoles,
                        Email = model.Email,
                        UserName = model.UserName,
                        PhoneNumber = model.PhoneNumber,
                        SelectedRoles = model.Roles,
                        SendConfirmationEmail = model.SendConfirmationEmail,
                        RequirePasswordChange = model.RequirePasswordChange
                    };
                    
                    return View(viewModel);
                }

                var result = await _adminUserService.CreateUserAsync(model);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "کاربر با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
                }

                ModelState.AddModelError("", result.ErrorMessage ?? "خطا در ایجاد کاربر");
                
                // Get available roles again for the form
                var rolesResult2 = await _adminUserService.GetAvailableRolesAsync();
                var availableRoles2 = rolesResult2.IsSuccess ? rolesResult2.Data! : new List<string>();
                
                var viewModel2 = new
                {
                    AvailableRoles = availableRoles2,
                    Email = model.Email,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    SelectedRoles = model.Roles,
                    SendConfirmationEmail = model.SendConfirmationEmail,
                    RequirePasswordChange = model.RequirePasswordChange
                };
                
                return View(viewModel2);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while creating user");
                return AdminError("خطا در ایجاد کاربر", ex.Message);
            }
        }

        /// <summary>
        /// Displays edit user form
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                SetAdminViewData();

                // Get user details
                var userResult = await _adminUserService.GetUserByIdAsync(id);
                if (!userResult.IsSuccess)
                {
                    return NotFound();
                }

                // Check if user can edit this user
                if (!IsSuperAdmin && userResult.Data!.Id != CurrentAdminId)
                {
                    return Forbid();
                }

                // Get available roles
                var rolesResult = await _adminUserService.GetAvailableRolesAsync();
                var availableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<string>();

                var viewModel = new
                {
                    UserId = userResult.Data!.Id,
                    Email = userResult.Data!.Email,
                    UserName = userResult.Data!.UserName,
                    PhoneNumber = userResult.Data!.PhoneNumber,
                    SelectedRoles = userResult.Data!.Roles ?? new List<string>(),
                    IsActive = userResult.Data!.IsActive,
                    IsEmailConfirmed = userResult.Data!.EmailConfirmed,
                    IsPhoneNumberConfirmed = userResult.Data!.PhoneNumberConfirmed,
                    AvailableRoles = availableRoles,
                    CreatedAt = userResult.Data!.CreatedAt,
                    LastLoginAt = userResult.Data!.LastLoginAt
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while loading edit user form for ID: {UserId}", id);
                return AdminError("خطا در بارگذاری فرم ویرایش کاربر", ex.Message);
            }
        }

        /// <summary>
        /// Handles edit user form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto model, string userId, List<string> selectedRoles)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Get available roles again for the form
                    var rolesResult = await _adminUserService.GetAvailableRolesAsync();
                    var availableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<string>();
                    
                    var viewModel = new
                    {
                        UserId = userId,
                        Email = model.Email,
                        UserName = model.UserName,
                        PhoneNumber = model.PhoneNumber,
                        SelectedRoles = selectedRoles,
                        IsActive = model.IsActive,
                        IsEmailConfirmed = model.EmailConfirmed,
                        IsPhoneNumberConfirmed = model.PhoneNumberConfirmed,
                        AvailableRoles = availableRoles
                    };
                    
                    return View(viewModel);
                }

                var result = await _adminUserService.UpdateUserAsync(userId, model);

                if (result.IsSuccess)
                {
                    // Update roles if they changed
                    var rolesResult = await _adminUserService.AssignRolesAsync(userId, selectedRoles);
                    
                    TempData["SuccessMessage"] = "کاربر با موفقیت ویرایش شد";
                    return RedirectToAction(nameof(Details), new { id = userId });
                }

                ModelState.AddModelError("", result.ErrorMessage ?? "خطا در ویرایش کاربر");
                
                // Get available roles again for the form
                var rolesResult2 = await _adminUserService.GetAvailableRolesAsync();
                var availableRoles2 = rolesResult2.IsSuccess ? rolesResult2.Data! : new List<string>();
                
                var viewModel2 = new
                {
                    UserId = userId,
                    Email = model.Email,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    SelectedRoles = selectedRoles,
                    IsActive = model.IsActive,
                    IsEmailConfirmed = model.EmailConfirmed,
                    IsPhoneNumberConfirmed = model.PhoneNumberConfirmed,
                    AvailableRoles = availableRoles2
                };
                
                return View(viewModel2);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while editing user");
                return AdminError("خطا در ویرایش کاربر", ex.Message);
            }
        }

        /// <summary>
        /// Handles user deletion
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                // Only SuperAdmin can delete users, and cannot delete themselves
                if (!IsSuperAdmin || id == CurrentAdminId)
                {
                    return Forbid();
                }

                var result = await _adminUserService.DeleteUserAsync(id);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "کاربر با موفقیت حذف شد";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = result.ErrorMessage ?? "خطا در حذف کاربر";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while deleting user with ID: {UserId}", id);
                TempData["ErrorMessage"] = "خطا در حذف کاربر";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}