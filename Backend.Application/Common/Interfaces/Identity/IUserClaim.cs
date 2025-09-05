namespace Backend.Application.Common.Interfaces.Identity;

/// <summary>
/// Interface for UserClaim to avoid dependency on Identity layer
/// </summary>
public interface IUserClaim
{
    int Id { get; }
    string UserId { get; }
    string ClaimType { get; }
    string ClaimValue { get; }
}