using Backend.Domain.Aggregates.Common;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects.Common;

namespace Backend.Domain.Entities.MFA;

/// <summary>
/// Represents a user's MFA method (TOTP, SMS, etc.)
/// </summary>
public class MfaMethod : BaseAggregateRoot<Guid>
{
    public string UserId { get; private set; }
    public MfaType Type { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime? LastUsed { get; private set; }
    public int FailedAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public AuditInfo AuditInfo { get; private set; }

    // TOTP specific properties
    public string? TotpSecretKey { get; private set; }
    public string? TotpQrCodeUrl { get; private set; }
    public int TotpDigits { get; private set; } = 6;
    public int TotpPeriod { get; private set; } = 30;

    // SMS specific properties
    public string? PhoneNumber { get; private set; }
    public DateTime? SmsCodeExpiry { get; private set; }
    public string? LastSmsCode { get; private set; }

    // Backup codes
    public List<string> BackupCodes { get; private set; } = new();
    public int RemainingBackupCodes { get; private set; }

    private MfaMethod() { } // For EF Core

    public MfaMethod(
        string userId, 
        MfaType type, 
        AuditInfo auditInfo)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Type = type;
        AuditInfo = auditInfo ?? throw new ArgumentNullException(nameof(auditInfo));
        IsEnabled = false;
        FailedAttempts = 0;
    }

    public static MfaMethod CreateTotp(string userId, AuditInfo auditInfo)
    {
        var mfa = new MfaMethod(userId, MfaType.TOTP, auditInfo);
        mfa.GenerateTotpSecret();
        return mfa;
    }

    public static MfaMethod CreateSms(string userId, string phoneNumber, AuditInfo auditInfo)
    {
        var mfa = new MfaMethod(userId, MfaType.SMS, auditInfo);
        mfa.SetPhoneNumber(phoneNumber);
        return mfa;
    }

    public static MfaMethod CreateBackupCodes(string userId, AuditInfo auditInfo)
    {
        var mfa = new MfaMethod(userId, MfaType.BackupCodes, auditInfo);
        mfa.GenerateBackupCodes();
        return mfa;
    }

    public void Enable()
    {
        if (IsEnabled)
            throw new InvalidOperationException("MFA method is already enabled");

        IsEnabled = true;
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public void Disable()
    {
        if (!IsEnabled)
            throw new InvalidOperationException("MFA method is already disabled");

        IsEnabled = false;
        FailedAttempts = 0;
        LockedUntil = null;
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public void GenerateTotpSecret()
    {
        if (Type != MfaType.TOTP)
            throw new InvalidOperationException("Can only generate TOTP secret for TOTP type");

        // Generate a secure random secret key
        var secretBytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(secretBytes);
        }
        TotpSecretKey = Convert.ToBase64String(secretBytes);
        
        // Generate QR code URL for authenticator apps
        var issuer = "BackendApp";
        var accountName = UserId;
        var secret = TotpSecretKey;
        TotpQrCodeUrl = $"otpauth://totp/{issuer}:{accountName}?secret={secret}&issuer={issuer}&digits={TotpDigits}&period={TotpPeriod}";
        
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        if (Type != MfaType.SMS)
            throw new InvalidOperationException("Can only set phone number for SMS type");

        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public void GenerateBackupCodes()
    {
        if (Type != MfaType.BackupCodes)
            throw new InvalidOperationException("Can only generate backup codes for BackupCodes type");

        BackupCodes.Clear();
        for (int i = 0; i < 10; i++)
        {
            var code = GenerateSecureBackupCode();
            BackupCodes.Add(code);
        }
        RemainingBackupCodes = BackupCodes.Count;
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public bool ValidateBackupCode(string code)
    {
        if (Type != MfaType.BackupCodes)
            return false;

        var index = BackupCodes.IndexOf(code);
        if (index >= 0)
        {
            BackupCodes.RemoveAt(index);
            RemainingBackupCodes--;
            AuditInfo = AuditInfo.UpdateModified(UserId);
            OnUpdated();
            return true;
        }
        return false;
    }

    public void RecordSuccessfulAttempt()
    {
        FailedAttempts = 0;
        LockedUntil = null;
        LastUsed = DateTime.UtcNow;
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public void RecordFailedAttempt()
    {
        FailedAttempts++;
        if (FailedAttempts >= 5) // Lock after 5 failed attempts
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }
        AuditInfo = AuditInfo.UpdateModified(UserId);
        OnUpdated();
    }

    public bool IsLocked()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    public bool CanAttempt()
    {
        return !IsLocked();
    }

    private string GenerateSecureBackupCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
} 