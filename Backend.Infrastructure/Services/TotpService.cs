using Backend.Application.Common.Interfaces.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Implementation of TOTP service using RFC 6238 standard
/// </summary>
public class TotpService : ITotpService
{
    private const int DefaultDigits = 6;
    private const int DefaultPeriod = 30; // 30 seconds
    private const string DefaultIssuer = "BackendApp";

    public string GenerateSecretKey()
    {
        var secretBytes = new byte[32]; // 256 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(secretBytes);
        return Convert.ToBase64String(secretBytes);
    }

    public string GenerateCode(string secretKey)
    {
        return GenerateCode(secretKey, DateTime.UtcNow);
    }

    public string GenerateCode(string secretKey, DateTime time)
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentException("Secret key cannot be null or empty", nameof(secretKey));

        var timeStep = GetTimeStep(time);
        var hash = GenerateHmacSha1(secretKey, timeStep);
        var code = GenerateCodeFromHash(hash, DefaultDigits);
        
        return code;
    }

    public bool ValidateCode(string secretKey, string code, int window = 1)
    {
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(code))
            return false;

        var currentTime = DateTime.UtcNow;
        
        // Check current time and previous/next windows
        for (int i = -window; i <= window; i++)
        {
            var checkTime = currentTime.AddSeconds(i * DefaultPeriod);
            var expectedCode = GenerateCode(secretKey, checkTime);
            
            if (string.Equals(expectedCode, code, StringComparison.Ordinal))
                return true;
        }
        
        return false;
    }

    public string GenerateQrCodeUrl(string secretKey, string accountName, string issuer = null)
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentException("Secret key cannot be null or empty", nameof(secretKey));

        if (string.IsNullOrEmpty(accountName))
            throw new ArgumentException("Account name cannot be null or empty", nameof(accountName));

        issuer ??= DefaultIssuer;

        // Format: otpauth://totp/{issuer}:{accountName}?secret={secret}&issuer={issuer}&digits={digits}&period={period}
        var url = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}" +
                  $"?secret={Uri.EscapeDataString(secretKey)}" +
                  $"&issuer={Uri.EscapeDataString(issuer)}" +
                  $"&digits={DefaultDigits}" +
                  $"&period={DefaultPeriod}";

        return url;
    }

    public int GetRemainingSeconds()
    {
        var now = DateTime.UtcNow;
        var timeStep = GetTimeStep(now);
        var nextStep = (timeStep + 1) * DefaultPeriod;
        var secondsSinceEpoch = (long)(now - DateTime.UnixEpoch).TotalSeconds;
        
        return (int)(nextStep - secondsSinceEpoch);
    }

    private long GetTimeStep(DateTime time)
    {
        var secondsSinceEpoch = (long)(time - DateTime.UnixEpoch).TotalSeconds;
        return secondsSinceEpoch / DefaultPeriod;
    }

    private byte[] GenerateHmacSha1(string secretKey, long timeStep)
    {
        var secretBytes = Convert.FromBase64String(secretKey);
        var timeStepBytes = BitConverter.GetBytes(timeStep);
        
        // Ensure big-endian byte order
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeStepBytes);

        using var hmac = new HMACSHA1(secretBytes);
        return hmac.ComputeHash(timeStepBytes);
    }

    private string GenerateCodeFromHash(byte[] hash, int digits)
    {
        // Get the last 4 bits of the hash
        var offset = hash[^1] & 0x0F;
        
        // Generate 4 bytes from the hash starting at the offset
        var code = ((hash[offset] & 0x7F) << 24) |
                   ((hash[offset + 1] & 0xFF) << 16) |
                   ((hash[offset + 2] & 0xFF) << 8) |
                   (hash[offset + 3] & 0xFF);
        
        // Convert to the specified number of digits
        var modulus = (int)Math.Pow(10, digits);
        var result = code % modulus;
        
        return result.ToString().PadLeft(digits, '0');
    }
} 