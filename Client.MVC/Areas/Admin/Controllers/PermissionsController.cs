using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.DTOs.Identity;
using Client.MVC.Services.Abstractions;
using Client.MVC.ViewModels.Admin;
using Client.MVC.Services.ApiClients;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for permission management in admin panel
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class PermissionsController : AdminBaseController
    {
        private readonly IUserApiClient _userApiClient;

        public PermissionsController(
            IUserApiClient userApiClient,
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger<PermissionsController> logger)
            : base(currentUser, antiForgeryService, logger)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
        }

        /// <summary>
        /// Displays the list of permissions
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                SetAdminViewData();

                var permissions = await _userApiClient.GetAllPermissionsAsync();
                
                var viewModel = new
                {
                    Permissions = permissions ?? new List<PermissionDto>(),
                    CanCreate = IsSuperAdmin,
                    CanEdit = IsSuperAdmin,
                    CanDelete = IsSuperAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting permissions for admin panel");
                TempData["ErrorMessage"] = "خطا در دریافت مجوزها";
                return View(new { Permissions = new List<PermissionDto>() });
            }
        }

        /// <summary>
        /// Displays permission details
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                SetAdminViewData();

                var permission = await _userApiClient.GetPermissionByIdAsync(id);
                if (permission == null)
                {
                    TempData["ErrorMessage"] = "مجوز مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new
                {
                    Permission = permission,
                    CanEdit = IsSuperAdmin,
                    CanDelete = IsSuperAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting permission details: {PermissionId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت جزئیات مجوز";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays create permission form
        /// </summary>
        public IActionResult Create()
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ایجاد مجوز ندارید";
                return RedirectToAction(nameof(Index));
            }

            SetAdminViewData();
            return View();
        }

        /// <summary>
        /// Creates a new permission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePermissionRequest model)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ایجاد مجوز ندارید";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                SetAdminViewData();
                return View(model);
            }

            try
            {
                var permission = await _userApiClient.CreatePermissionAsync(model);
                
                if (permission != null)
                {
                    TempData["SuccessMessage"] = "مجوز با موفقیت ایجاد شد";
                    return RedirectToAction(nameof(Details), new { id = permission.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در ایجاد مجوز";
                    SetAdminViewData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while creating permission: {PermissionName}", model.Name);
                TempData["ErrorMessage"] = "خطا در ایجاد مجوز";
                SetAdminViewData();
                return View(model);
            }
        }

        /// <summary>
        /// Displays edit permission form
        /// </summary>
        public async Task<IActionResult> Edit(string id)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ویرایش مجوز ندارید";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                SetAdminViewData();

                var permission = await _userApiClient.GetPermissionByIdAsync(id);
                if (permission == null)
                {
                    TempData["ErrorMessage"] = "مجوز مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdatePermissionRequest
                {
                    Name = permission.Name,
                    Resource = permission.Resource,
                    Action = permission.Action,
                    Description = permission.Description,
                    Category = permission.Category,
                    Priority = permission.Priority,
                    IsSystemPermission = permission.IsSystemPermission,
                    IsActive = permission.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting permission for edit: {PermissionId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت مجوز";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Updates a permission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdatePermissionRequest model)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای ویرایش مجوز ندارید";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                SetAdminViewData();
                return View(model);
            }

            try
            {
                var permission = await _userApiClient.UpdatePermissionAsync(id, model);
                
                if (permission != null)
                {
                    TempData["SuccessMessage"] = "مجوز با موفقیت به‌روزرسانی شد";
                    return RedirectToAction(nameof(Details), new { id = permission.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در به‌روزرسانی مجوز";
                    SetAdminViewData();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while updating permission: {PermissionId}", id);
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی مجوز";
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
                TempData["ErrorMessage"] = "شما دسترسی لازم برای حذف مجوز ندارید";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                SetAdminViewData();

                var permission = await _userApiClient.GetPermissionByIdAsync(id);
                if (permission == null)
                {
                    TempData["ErrorMessage"] = "مجوز مورد نظر یافت نشد";
                    return RedirectToAction(nameof(Index));
                }

                return View(permission);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting permission for delete: {PermissionId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت مجوز";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Deletes a permission
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!IsSuperAdmin)
            {
                TempData["ErrorMessage"] = "شما دسترسی لازم برای حذف مجوز ندارید";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var success = await _userApiClient.DeletePermissionAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "مجوز با موفقیت حذف شد";
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در حذف مجوز";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while deleting permission: {PermissionId}", id);
                TempData["ErrorMessage"] = "خطا در حذف مجوز";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}