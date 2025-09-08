using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.DTOs.Identity;
using Backend.Application.Common.Authorization;
using Client.MVC.Services.Abstractions;
using Client.MVC.ViewModels.Admin;
using Client.MVC.Services.ApiClients;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for role management in admin panel
    /// Only SuperAdmin can manage roles
    /// </summary>
    [Area("Admin")]
    [Authorize(Policy = RoleBasedAuthorizationPolicies.CanManageRoles)]
    public class RolesController : AdminBaseController
    {
        private readonly IUserApiClient _userApiClient;

        public RolesController(
            IUserApiClient userApiClient,
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger<RolesController> logger)
            : base(currentUser, antiForgeryService, logger)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
        }

        /// <summary>
        /// Displays the list of roles
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                SetAdminViewData();

                var roles = await _userApiClient.GetAllRolesAsync();
                
                var viewModel = new
                {
                    Roles = roles ?? new List<RoleDto>(),
                    CanCreate = IsSuperAdmin,
                    CanEdit = IsSuperAdmin,
                    CanDelete = IsSuperAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting roles for admin panel");
                TempData["ErrorMessage"] = "خطا در دریافت نقش‌ها";
                return View(new { Roles = new List<RoleDto>() });
            }
        }

        /// <summary>
        /// Displays role details
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                SetAdminViewData();

                var role = await _userApiClient.GetRoleByIdAsync(id);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "نقش مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                // Get role permissions
                var permissions = await _userApiClient.GetRolePermissionsAsync(id);

                var viewModel = new
                {
                    Role = role,
                    Permissions = permissions ?? new List<PermissionDto>(),
                    CanEdit = IsSuperAdmin,
                    CanDelete = IsSuperAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting role details: {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت جزئیات نقش";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays create role form
        /// </summary>
        public IActionResult Create()
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ایجاد نقش ندارید";
                return RedirectToAction(nameof(Index));
            }

            SetAdminViewData();
            return View();
        }

        /// <summary>
        /// Creates a new role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleRequest model)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ایجاد نقش ندارید";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                SetAdminViewData();
                return View(model);
            }

            try
            {
                var role = await _userApiClient.CreateRoleAsync(model);
                
                if (role != null)
                {
                    TempData["SuccessMessage"] = "نقش با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Details), new { id = role.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در ایجاد نقش";
                    SetAdminViewData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while creating role: {RoleName}", model.Name);
                TempData["ErrorMessage"] = "خطا در ایجاد نقش";
                SetAdminViewData();
                return View(model);
            }
        }

        /// <summary>
        /// Displays edit role form
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ویرایش نقش ندارید";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                SetAdminViewData();

                var role = await _userApiClient.GetRoleByIdAsync(id);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "نقش مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdateRoleRequest
                {
                    Name = role.Name,
                    Description = role.Description,
                    Category = role.Category,
                    Priority = role.Priority,
                    IsSystemRole = role.IsSystemRole,
                    IsActive = role.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting role for edit: {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت نقش";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Updates a role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateRoleRequest model)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ویرایش نقش ندارید";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                SetAdminViewData();
                return View(model);
            }

            try
            {
                var role = await _userApiClient.UpdateRoleAsync(id, model);
                
                if (role != null)
                {
                    TempData["SuccessMessage"] = "نقش با موفقیت به‌روزرسانی شد";
                    return RedirectToAction(nameof(Details), new { id = role.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در به‌روزرسانی نقش";
                    SetAdminViewData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while updating role: {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی نقش";
                SetAdminViewData();
                return View(model);
            }
        }

        /// <summary>
        /// Displays delete confirmation
        /// </summary>
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای حذف نقش ندارید";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                SetAdminViewData();

                var role = await _userApiClient.GetRoleByIdAsync(id);
                if (role == null)
                {
                    TempData["ErrorMessage"] = "نقش مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                return View(role);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting role for delete: {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت نقش";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Deletes a role
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای حذف نقش ندارید";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _userApiClient.DeleteRoleAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "نقش با موفقیت حذف شد";
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در حذف نقش";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while deleting role: {RoleId}", id);
                TempData["ErrorMessage"] = "خطا در حذف نقش";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Assigns permission to role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPermission(string roleId, string permissionId)
        {
            if (!IsSuperAdmin)
            {
                return Json(new { success = false, message = "شما دسترسی لازم ندارید" });
            }

            try
            {
                var success = await _userApiClient.AssignPermissionToRoleAsync(roleId, permissionId);
                
                if (success)
                {
                    return Json(new { success = true, message = "مجوز با موفقیت به نقش اضافه شد" });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در اضافه کردن مجوز به نقش" });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while assigning permission to role: {RoleId}, {PermissionId}", roleId, permissionId);
                return Json(new { success = false, message = "خطا در اضافه کردن مجوز به نقش" });
            }
        }

        /// <summary>
        /// Removes permission from role
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePermission(string roleId, string permissionId)
        {
            if (!IsSuperAdmin)
            {
                return Json(new { success = false, message = "شما دسترسی لازم ندارید" });
            }

            try
            {
                var success = await _userApiClient.RemovePermissionFromRoleAsync(roleId, permissionId);
                
                if (success)
                {
                    return Json(new { success = true, message = "مجوز با موفقیت از نقش حذف شد" });
                }
                else
                {
                    return Json(new { success = false, message = "خطا در حذف مجوز از نقش" });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while removing permission from role: {RoleId}, {PermissionId}", roleId, permissionId);
                return Json(new { success = false, message = "خطا در حذف مجوز از نقش" });
            }
        }
    }
}