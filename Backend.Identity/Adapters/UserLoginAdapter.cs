using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;

namespace Backend.Identity.Adapters;

/// <summary>
/// Adapter to convert UserLogin to IUserLogin interface
/// </summary>
public class UserLoginAdapter : IUserLogin
{
    private readonly UserLogin _userLogin;

    public UserLoginAdapter(UserLogin userLogin)
    {
        _userLogin = userLogin ?? throw new ArgumentNullException(nameof(userLogin));
    }

    public string LoginProvider => _userLogin.LoginProvider;
    public string ProviderKey => _userLogin.ProviderKey;
    public string ProviderDisplayName => _userLogin.ProviderDisplayName;
    public string UserId => _userLogin.UserId;
}