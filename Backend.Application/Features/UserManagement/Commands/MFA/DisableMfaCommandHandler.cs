using Backend.Application.Common.Commands;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Domain.Entities.MFA;
using Backend.Domain.Enums;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.ValueObjects.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Handler for DisableMfaCommand
/// </summary>
public class DisableMfaCommandHandler : ICommandHandler<DisableMfaCommand, bool>
{
    private readonly IMfaMethodRepository _mfaRepository;
    private readonly ILogger<DisableMfaCommandHandler> _logger;

    public DisableMfaCommandHandler(
        IMfaMethodRepository mfaRepository,
        ILogger<DisableMfaCommandHandler> logger)
    {
        _mfaRepository = mfaRepository ?? throw new ArgumentNullException(nameof(mfaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Disabling MFA for user {UserId}, type {Type}", request.UserId, request.Type);

            // Get MFA method
            var mfaMethod = await _mfaRepository.GetByUserIdAndTypeAsync(request.UserId, request.Type, cancellationToken);
            if (mfaMethod == null)
            {
                _logger.LogWarning("MFA method not found for user {UserId}, type {Type}", request.UserId, request.Type);
                return false;
            }

            // Check if MFA is already disabled
            if (!mfaMethod.IsEnabled)
            {
                _logger.LogInformation("MFA method is already disabled for user {UserId}, type {Type}", request.UserId, request.Type);
                return true;
            }

            // Disable MFA
            mfaMethod.Disable();

            // Update in database
            await _mfaRepository.UpdateAsync(mfaMethod, cancellationToken);

            _logger.LogInformation("MFA disabled successfully for user {UserId}, type {Type}", request.UserId, request.Type);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA for user {UserId}, type {Type}", request.UserId, request.Type);
            throw;
        }
    }
} 