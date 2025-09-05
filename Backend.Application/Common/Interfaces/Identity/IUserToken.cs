using System;

namespace Backend.Application.Common.Interfaces.Identity;

/// <summary>
/// Interface for UserToken to avoid dependency on Identity layer
/// </summary>
public interface IUserToken
{
    int Id { get; }
    string UserId { get; }
    string LoginProvider { get; }
    string Name { get; }
    string Value { get; }
    DateTime? ExpiresAt { get; }
    bool IsActive { get; }
}