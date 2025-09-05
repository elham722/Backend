namespace Client.MVC.Services.Abstractions
{
    public interface IJwtClaimsExtractor
    {
        string? GetUserId(string? jwtToken);
        string? GetUserName(string? jwtToken);
        string? GetUserEmail(string? jwtToken);
        IEnumerable<string> GetUserRoles(string? jwtToken);
        DateTime? GetTokenExpiration(string? jwtToken);
        bool IsTokenValid(string? jwtToken);
        IDictionary<string, object> GetAllClaims(string? jwtToken);

        // ✅ New methods for generic claim extraction
        string? GetClaimValue(string claimType, string? jwtToken = null);
        IEnumerable<string> GetClaimValues(string claimType, string? jwtToken = null);
    }

}
