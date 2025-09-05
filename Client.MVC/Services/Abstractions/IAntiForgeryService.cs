namespace Client.MVC.Services.Abstractions
{
    public interface IAntiForgeryService
    {
        string GetToken();
        string GetTokenHeaderName();
        string GetTokenCookieName();
        bool ValidateToken(string token);
        void SetTokenInCookie(HttpContext context);
        string GetTokenFromCookie(HttpContext context);
    }
}
