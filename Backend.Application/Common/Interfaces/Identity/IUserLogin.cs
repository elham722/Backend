namespace Backend.Application.Common.Interfaces.Identity;

/// <summary>
/// Interface for UserLogin to avoid dependency on Identity layer
/// </summary>
public interface IUserLogin
{
    string LoginProvider { get; }
    string ProviderKey { get; }
    string ProviderDisplayName { get; }
    string UserId { get; }
}