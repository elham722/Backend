using Client.MVC.Services.Admin;
using Backend.Application.Features.UserManagement.DTOs;
using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for admin dashboard
    /// </summary>
    ///   [Area("Admin")]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : AdminBaseController
    {
        private readonly IAdminUserService _adminUserService;

        public DashboardController(
            IAdminUserService adminUserService,
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger<DashboardController> logger)
            : base(currentUser, antiForgeryService, logger)
        {
            _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
        }

        /// <summary>
        /// Displays the admin dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                SetAdminViewData();

                // Get user statistics
                var statisticsResult = await _adminUserService.GetUserStatisticsAsync();
                var statistics = statisticsResult.IsSuccess ? statisticsResult.Data! : new AdminDashboardStatsDto();

                // Get recent users (first 5)
                var usersResult = await _adminUserService.GetUsersAsync(1, 5);
                List<UserDto> recentUsers;

                if (usersResult.IsSuccess && usersResult.Data != null)
                {
                    recentUsers = usersResult.Data.ToList();
                }
                else
                {
                    recentUsers = new List<UserDto>();
                }

                // Create a simple model for the view
                var viewModel = new
                {
                    Statistics = statistics,
                    RecentActivity = statistics.RecentActivity,
                    RecentUsers = recentUsers,
                    IsSuperAdmin = IsSuperAdmin,
                    AdminUsername = CurrentAdminUsername
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while loading admin dashboard");
                return AdminError("خطا در بارگذاری داشبورد ادمین", ex.Message);
            }
        }

        /// <summary>
        /// Gets dashboard statistics via AJAX
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var result = await _adminUserService.GetUserStatisticsAsync();
                
                if (result.IsSuccess)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while getting dashboard statistics");
                return Json(new { success = false, message = "خطا در دریافت آمار" });
            }
        }
    }
}