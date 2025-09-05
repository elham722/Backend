using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;

namespace Backend.Identity.Adapters;

/// <summary>
/// Adapter to convert UserClaim to IUserClaim interface
/// </summary>
public class UserClaimAdapter : IUserClaim
{
    private readonly UserClaim _userClaim;

    public UserClaimAdapter(UserClaim userClaim)
    {
        _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
    }

    public int Id => _userClaim.Id;
    public string UserId => _userClaim.UserId;
    public string ClaimType => _userClaim.ClaimType;
    public string ClaimValue => _userClaim.ClaimValue;
}