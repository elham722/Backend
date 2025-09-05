using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;

namespace Backend.Identity.Adapters;

/// <summary>
/// Adapter to convert UserToken to IUserToken interface
/// </summary>
public class UserTokenAdapter : IUserToken
{
    private readonly UserToken _userToken;

    public UserTokenAdapter(UserToken userToken)
    {
        _userToken = userToken ?? throw new ArgumentNullException(nameof(userToken));
    }

    public string UserId => _userToken.UserId;
    public string LoginProvider => _userToken.LoginProvider;
    public string Name => _userToken.Name;
    public string Value => _userToken.Value;
    public DateTime? ExpiresAt => _userToken.ExpiresAt;
    public bool IsActive => _userToken.IsActive;

    public int Id => throw new NotImplementedException();
}