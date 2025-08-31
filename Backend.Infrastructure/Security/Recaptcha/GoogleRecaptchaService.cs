using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Backend.Application.Common.Security;

namespace Backend.Infrastructure.Security.Recaptcha;

/// <summary>
/// Google reCAPTCHA service implementation
/// </summary>
internal sealed class GoogleRecaptchaService : IHumanVerificationService
{
    private readonly HttpClient _http;
    private readonly RecaptchaOptions _opt;
    private readonly ILogger<GoogleRecaptchaService> _logger;

    public GoogleRecaptchaService(
        HttpClient http,
        IOptions<RecaptchaOptions> options,
        ILogger<GoogleRecaptchaService> logger)
    {
        _http = http;
        _opt = options.Value;
        _logger = logger;
    }

    public async Task<CaptchaVerificationResult> VerifyAsync(
        string token, 
        string? action = null, 
        string? userIp = null, 
        CancellationToken ct = default)
    {
        // Check if reCAPTCHA is enabled
        if (!_opt.Enabled)
        {
            _logger.LogDebug("reCAPTCHA is disabled");
            return new CaptchaVerificationResult(true);
        }

        // Bypass in development if configured
        if (_opt.BypassInDevelopment)
        {
            _logger.LogInformation("reCAPTCHA bypassed in development environment");
            return new CaptchaVerificationResult(true);
        }

        // Validate token
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("CAPTCHA token is missing");
            return new CaptchaVerificationResult(false, ErrorCodes: new[] { "missing-input-response" });
        }

        try
        {
            _logger.LogDebug("Verifying reCAPTCHA token. Length: {TokenLength}, Action: {Action}", 
                token.Length, action ?? "none");

            // Prepare verification request
            var formData = new Dictionary<string, string?>
            {
                ["secret"] = _opt.SecretKey,
                ["response"] = token
            };

            if (!string.IsNullOrWhiteSpace(userIp))
            {
                formData["remoteip"] = userIp;
            }

            var form = new FormUrlEncodedContent(formData.Where(kv => kv.Value != null)!);

            // Send verification request to Google
            using var resp = await _http.PostAsync(_opt.VerifyUrl, form, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            _logger.LogDebug("Google reCAPTCHA response: {Response}", json);

            // Parse Google's response
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            bool success = root.GetProperty("success").GetBoolean();
            string? respAction = root.TryGetProperty("action", out var act) ? act.GetString() : null;
            string? hostname = root.TryGetProperty("hostname", out var host) ? host.GetString() : null;
            double? score = root.TryGetProperty("score", out var sc) ? sc.GetDouble() : null;

            var errors = root.TryGetProperty("error-codes", out var codes) && codes.ValueKind == JsonValueKind.Array
                ? codes.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray()
                : Array.Empty<string>();

            if (!success)
            {
                _logger.LogWarning("reCAPTCHA verification failed. Errors: {Errors}", errors);
                return new CaptchaVerificationResult(false, Score: score, Action: respAction, Hostname: hostname, ErrorCodes: errors);
            }

            // For v3, validate action and score
            if (_opt.Version == RecaptchaVersion.V3)
            {
                if (!string.IsNullOrWhiteSpace(action) && !string.Equals(action, respAction, StringComparison.Ordinal))
                {
                    _logger.LogWarning("reCAPTCHA action mismatch. Expected: {Expected}, Got: {Got}", action, respAction);
                    return new CaptchaVerificationResult(false, Score: score, Action: respAction, Hostname: hostname, ErrorCodes: new[] { "action-mismatch" });
                }

                if (score is null || score < _opt.MinimumScore)
                {
                    _logger.LogWarning("reCAPTCHA score too low. Score: {Score}, Minimum: {Minimum}", score, _opt.MinimumScore);
                    return new CaptchaVerificationResult(false, Score: score, Action: respAction, Hostname: hostname, ErrorCodes: new[] { "low-score" });
                }
            }

            _logger.LogInformation("reCAPTCHA verification successful. Score: {Score}, Action: {Action}", score, respAction);
            return new CaptchaVerificationResult(true, Score: score, Action: respAction, Hostname: hostname);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reCAPTCHA verification");
            return new CaptchaVerificationResult(false, ErrorCodes: new[] { "verification-error" });
        }
    }
} 