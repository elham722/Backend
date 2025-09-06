using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.ViewComponents
{
    public class UserStatusViewComponent : ViewComponent
    {
        private readonly IUserSessionService _userSessionService;

        public UserStatusViewComponent(IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;
        }

        public IViewComponentResult Invoke()
        {
            var isAuthenticated = _userSessionService.IsAuthenticated();
            var userName = _userSessionService.GetUserName();
            var userEmail = _userSessionService.GetUserEmail();
            var userRoles = _userSessionService.GetUserRoles();

            var model = new UserStatusViewModel
            {
                IsAuthenticated = isAuthenticated,
                UserName = userName,
                UserEmail = userEmail,
                UserRoles = userRoles?.ToList() ?? new List<string>(),
                IsAdmin = userRoles?.Contains("Admin") == true || userRoles?.Contains("SuperAdmin") == true
            };

            return View(model);
        }
    }

    public class UserStatusViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public List<string> UserRoles { get; set; } = new();
        public bool IsAdmin { get; set; }
    }
} 