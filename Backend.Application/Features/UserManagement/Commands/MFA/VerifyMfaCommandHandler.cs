using Backend.Application.Common.Commands;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Domain.Entities.MFA;
using Backend.Domain.Enums;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.ValueObjects.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Handler for VerifyMfaCommand
/// </summary>
public class VerifyMfaCommandHandler : ICommandHandler<VerifyMfaCommand, MfaVerificationResultDto>
{
    private readonly IMfaMethodRepository _mfaRepository;
    private readonly ITotpService _totpService;
    private readonly ISmsService _smsService;
    private readonly ILogger<VerifyMfaCommandHandler> _logger;

    public VerifyMfaCommandHandler(
        IMfaMethodRepository mfaRepository,
        ITotpService totpService,
        ISmsService smsService,
        ILogger<VerifyMfaCommandHandler> logger)
    {
        _mfaRepository = mfaRepository ?? throw new ArgumentNullException(nameof(mfaRepository));
        _totpService = totpService ?? throw new ArgumentNullException(nameof(totpService));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MfaVerificationResultDto> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Verifying MFA for user {UserId}, type {Type}", request.UserId, request.Type);

            // Get MFA method
            var mfaMethod = await _mfaRepository.GetByUserIdAndTypeAsync(request.UserId, request.Type, cancellationToken);
            if (mfaMethod == null)
            {
                _logger.LogWarning("MFA method not found for user {UserId}, type {Type}", request.UserId, request.Type);
                return new MfaVerificationResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "MFA method not found",
                    IsValid = false
                };
            }

            // Check if MFA is enabled
            if (!mfaMethod.IsEnabled)
            {
                _logger.LogWarning("MFA method is not enabled for user {UserId}, type {Type}", request.UserId, request.Type);
                return new MfaVerificationResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "MFA method is not enabled",
                    IsValid = false
                };
            }

            // Check if MFA is locked
            if (mfaMethod.IsLocked())
            {
                _logger.LogWarning("MFA method is locked for user {UserId}, type {Type}", request.UserId, request.Type);
                return new MfaVerificationResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "MFA method is temporarily locked due to too many failed attempts",
                    IsValid = false
                };
            }

            // Validate code based on MFA type
            bool isValid = request.Type switch
            {
                MfaType.TOTP => ValidateTotpCode(mfaMethod, request.Code),
                MfaType.SMS => ValidateSmsCode(mfaMethod, request.Code),
                MfaType.BackupCodes => ValidateBackupCode(mfaMethod, request.Code),
                _ => false
            };

            if (isValid)
            {
                // Record successful attempt
                mfaMethod.RecordSuccessfulAttempt();
                await _mfaRepository.UpdateAsync(mfaMethod, cancellationToken);

                _logger.LogInformation("MFA verification successful for user {UserId}, type {Type}", request.UserId, request.Type);

                return new MfaVerificationResultDto
                {
                    IsSuccess = true,
                    IsValid = true,
                    RequiresAdditionalVerification = false
                };
            }
            else
            {
                // Record failed attempt
                mfaMethod.RecordFailedAttempt();
                await _mfaRepository.UpdateAsync(mfaMethod, cancellationToken);

                _logger.LogWarning("MFA verification failed for user {UserId}, type {Type}", request.UserId, request.Type);

                return new MfaVerificationResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid verification code",
                    IsValid = false
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA for user {UserId}, type {Type}", request.UserId, request.Type);
            throw;
        }
    }

    private bool ValidateTotpCode(MfaMethod mfaMethod, string code)
    {
        if (string.IsNullOrEmpty(mfaMethod.TotpSecretKey))
            return false;

        return _totpService.ValidateCode(mfaMethod.TotpSecretKey, code, window: 1);
    }

    private bool ValidateSmsCode(MfaMethod mfaMethod, string code)
    {
        if (string.IsNullOrEmpty(mfaMethod.LastSmsCode))
            return false;

        // Check if SMS code has expired
        if (mfaMethod.SmsCodeExpiry.HasValue && mfaMethod.SmsCodeExpiry.Value < DateTime.UtcNow)
            return false;

        // Compare codes (in production, you might want to hash the codes)
        return string.Equals(mfaMethod.LastSmsCode, code, StringComparison.Ordinal);
    }

    private bool ValidateBackupCode(MfaMethod mfaMethod, string code)
    {
        return mfaMethod.ValidateBackupCode(code);
    }
} 