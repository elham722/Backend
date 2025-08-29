using Client.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUserSessionService UserSessionService;
        protected readonly ILogger Logger;

        protected BaseController(IUserSessionService userSessionService, ILogger logger)
        {
            UserSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected string? CurrentUserId => UserSessionService.GetUserId();

        protected string? CurrentUserName => UserSessionService.GetUserName();

        protected string? CurrentUserEmail => UserSessionService.GetUserEmail();

        protected bool IsAuthenticated => UserSessionService.IsAuthenticated();

        protected string? JwtToken => UserSessionService.GetJwtToken();

        protected void SetUserViewBag()
        {
            ViewBag.IsLoggedIn = IsAuthenticated;
            ViewBag.UserName = CurrentUserName;
            ViewBag.UserEmail = CurrentUserEmail;
            ViewBag.UserId = CurrentUserId;
        }
    }
} 