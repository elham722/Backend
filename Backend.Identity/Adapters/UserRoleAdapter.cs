using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;

namespace Backend.Identity.Adapters;

/// <summary>
/// Adapter to convert UserRole to IUserRole interface
/// </summary>
public class UserRoleAdapter : IUserRole
{
    private readonly UserRole _userRole;

    public UserRoleAdapter(UserRole userRole)
    {
        _userRole = userRole ?? throw new ArgumentNullException(nameof(userRole));
    }

    public string UserId => _userRole.UserId;
    public string RoleId => _userRole.RoleId;

    public string RoleName => throw new NotImplementedException();
}