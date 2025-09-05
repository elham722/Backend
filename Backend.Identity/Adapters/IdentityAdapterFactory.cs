using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;

namespace Backend.Identity.Adapters;

/// <summary>
/// Factory for creating Identity adapters
/// This factory provides a centralized way to convert Identity models to Application interfaces
/// </summary>
public static class IdentityAdapterFactory
{
    /// <summary>
    /// Creates an IApplicationUser adapter from ApplicationUser
    /// </summary>
    /// <param name="user">The ApplicationUser instance</param>
    /// <returns>IApplicationUser adapter</returns>
    public static IApplicationUser CreateApplicationUser(ApplicationUser user)
    {
        return new ApplicationUserAdapter(user);
    }

    /// <summary>
    /// Creates an IUserClaim adapter from UserClaim
    /// </summary>
    /// <param name="userClaim">The UserClaim instance</param>
    /// <returns>IUserClaim adapter</returns>
    public static IUserClaim CreateUserClaim(UserClaim userClaim)
    {
        return new UserClaimAdapter(userClaim);
    }

    /// <summary>
    /// Creates an IUserToken adapter from UserToken
    /// </summary>
    /// <param name="userToken">The UserToken instance</param>
    /// <returns>IUserToken adapter</returns>
    public static IUserToken CreateUserToken(UserToken userToken)
    {
        return new UserTokenAdapter(userToken);
    }

    /// <summary>
    /// Creates an IUserRole adapter from UserRole
    /// </summary>
    /// <param name="userRole">The UserRole instance</param>
    /// <returns>IUserRole adapter</returns>
    public static IUserRole CreateUserRole(UserRole userRole)
    {
        return new UserRoleAdapter(userRole);
    }

    /// <summary>
    /// Creates an IUserLogin adapter from UserLogin
    /// </summary>
    /// <param name="userLogin">The UserLogin instance</param>
    /// <returns>IUserLogin adapter</returns>
    public static IUserLogin CreateUserLogin(UserLogin userLogin)
    {
        return new UserLoginAdapter(userLogin);
    }

    /// <summary>
    /// Creates a collection of IApplicationUser adapters from a collection of ApplicationUser
    /// </summary>
    /// <param name="users">Collection of ApplicationUser instances</param>
    /// <returns>Collection of IApplicationUser adapters</returns>
    public static IEnumerable<IApplicationUser> CreateApplicationUsers(IEnumerable<ApplicationUser> users)
    {
        return users.Select(CreateApplicationUser);
    }

    /// <summary>
    /// Creates a collection of IUserClaim adapters from a collection of UserClaim
    /// </summary>
    /// <param name="userClaims">Collection of UserClaim instances</param>
    /// <returns>Collection of IUserClaim adapters</returns>
    public static IEnumerable<IUserClaim> CreateUserClaims(IEnumerable<UserClaim> userClaims)
    {
        return userClaims.Select(CreateUserClaim);
    }

    /// <summary>
    /// Creates a collection of IUserToken adapters from a collection of UserToken
    /// </summary>
    /// <param name="userTokens">Collection of UserToken instances</param>
    /// <returns>Collection of IUserToken adapters</returns>
    public static IEnumerable<IUserToken> CreateUserTokens(IEnumerable<UserToken> userTokens)
    {
        return userTokens.Select(CreateUserToken);
    }

    /// <summary>
    /// Creates a collection of IUserRole adapters from a collection of UserRole
    /// </summary>
    /// <param name="userRoles">Collection of UserRole instances</param>
    /// <returns>Collection of IUserRole adapters</returns>
    public static IEnumerable<IUserRole> CreateUserRoles(IEnumerable<UserRole> userRoles)
    {
        return userRoles.Select(CreateUserRole);
    }

    /// <summary>
    /// Creates a collection of IUserLogin adapters from a collection of UserLogin
    /// </summary>
    /// <param name="userLogins">Collection of UserLogin instances</param>
    /// <returns>Collection of IUserLogin adapters</returns>
    public static IEnumerable<IUserLogin> CreateUserLogins(IEnumerable<UserLogin> userLogins)
    {
        return userLogins.Select(CreateUserLogin);
    }
}