namespace Backend.Application.Common.Interfaces.Identity;

/// <summary>
/// Interface for UserRole to avoid dependency on Identity layer
/// </summary>
public interface IUserRole
{
    string UserId { get; }
    string RoleId { get; }
    string RoleName { get; }
}