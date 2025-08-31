using Backend.Application.Common.Queries;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Features.UserManagement.Queries.MFA;

/// <summary>
/// Query to get MFA methods for a user
/// </summary>
public class GetMfaMethodsQuery : IQuery<IEnumerable<MfaSetupDto>>
{
    public string UserId { get; set; } = string.Empty;
    public bool? IsEnabled { get; set; }
} 