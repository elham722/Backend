using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;
using Backend.Identity.ValueObjects;

namespace Backend.Identity.Adapters;

/// <summary>
/// Adapter to convert ApplicationUser to IApplicationUser interface
/// This allows the Application layer to work with Identity models without direct dependency
/// </summary>
public class ApplicationUserAdapter : IApplicationUser
{
    private readonly ApplicationUser _user;

    public ApplicationUserAdapter(ApplicationUser user)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
    }

    // Core Identity Properties
    public string Id => _user.Id;
    public string Email => _user.Email ?? string.Empty;
    public string UserName => _user.UserName ?? string.Empty;
    public string? PhoneNumber => _user.PhoneNumber;
    public bool EmailConfirmed => _user.EmailConfirmed;
    public bool PhoneNumberConfirmed => _user.PhoneNumberConfirmed;
    public bool TwoFactorEnabled => _user.TwoFactorEnabled;
    public bool LockoutEnabled => _user.LockoutEnabled;
    public int AccessFailedCount => _user.AccessFailedCount;
    public DateTimeOffset? LockoutEnd => _user.LockoutEnd;
    
    // Custom Properties
    public Guid? CustomerId => _user.CustomerId;
    public string? TotpSecretKey => _user.TotpSecretKey;
    public bool TotpEnabled => _user.TotpEnabled;
    public bool SmsEnabled => _user.SmsEnabled;
    public string? GoogleId => _user.GoogleId;
    public string? MicrosoftId => _user.MicrosoftId;
    
    // Account Info
    public IAccountInfo Account => new AccountInfoAdapter(_user.Account);
    
    // Security Info
    public ISecurityInfo Security => new SecurityInfoAdapter(_user.Security);
    
    // Audit Info
    public IAuditInfo Audit => new AuditInfoAdapter(_user.Audit, _user.Account.CreatedAt);
    
    // Navigation Properties
    public List<string> Roles => _user.Roles;
    
    // Computed Properties
    public bool IsLocked => _user.IsLocked;
    public bool IsAccountLocked => _user.IsAccountLocked;
    public bool IsActive => _user.IsActive;
    public bool IsDeleted => _user.Account.IsDeleted;
    public bool IsNewUser => _user.IsNewUser;
    
    // Additional Properties for DTO mapping
    public DateTime? LastLoginAt => _user.Account.LastLoginAt;
    public DateTime? LastPasswordChangeAt => _user.Account.LastPasswordChangeAt;
    public int LoginAttempts => _user.Account.LoginAttempts;

    bool IApplicationUser.RequiresPasswordChange => _user.RequiresPasswordChange;

    // Business Logic Methods
    public bool CanLogin() => _user.CanLogin();
}

/// <summary>
/// Adapter for AccountInfo value object
/// </summary>
public class AccountInfoAdapter : IAccountInfo
{
    private readonly AccountInfo _accountInfo;

    public AccountInfoAdapter(AccountInfo accountInfo)
    {
        _accountInfo = accountInfo ?? throw new ArgumentNullException(nameof(accountInfo));
    }

    public bool IsActive => _accountInfo.IsActive;
    public bool IsDeleted => _accountInfo.IsDeleted;
    public bool IsLocked() => _accountInfo.IsLocked();
    public DateTime CreatedAt => _accountInfo.CreatedAt;
    public DateTime? LastPasswordChangeAt => _accountInfo.LastPasswordChangeAt;
    public DateTime? LastLoginAt => _accountInfo.LastLoginAt;
    public int LoginAttempts => _accountInfo.LoginAttempts;
    public string? BranchId => _accountInfo.BranchId;
}

/// <summary>
/// Adapter for SecurityInfo value object
/// </summary>
public class SecurityInfoAdapter : ISecurityInfo
{
    private readonly SecurityInfo _securityInfo;

    public SecurityInfoAdapter(SecurityInfo securityInfo)
    {
        _securityInfo = securityInfo ?? throw new ArgumentNullException(nameof(securityInfo));
    }

    public string? SecurityQuestion => null; // SecurityInfo doesn't have SecurityQuestion

    public string? SecurityAnswer => null; // SecurityInfo doesn't have SecurityAnswer

    public DateTime? LastSecurityUpdate => _securityInfo.TwoFactorEnabledAt; // Map to TwoFactorEnabledAt
}

/// <summary>
/// Adapter for AuditInfo value object
/// </summary>
public class AuditInfoAdapter : IAuditInfo
{
    private readonly AuditInfo _auditInfo;
    private readonly DateTime _createdAt;

    public AuditInfoAdapter(AuditInfo auditInfo, DateTime createdAt)
    {
        _auditInfo = auditInfo ?? throw new ArgumentNullException(nameof(auditInfo));
        _createdAt = createdAt;
    }

    public string? CreatedBy => _auditInfo.CreatedBy;

    public DateTime CreatedAt => _createdAt; // Use provided CreatedAt

    public string? LastModifiedBy => _auditInfo.UpdatedBy; // Map to UpdatedBy

    public DateTime? LastModifiedAt => _auditInfo.UpdatedAt; // Map to UpdatedAt
}