using AutoMapper;
using Backend.Application.Common.Queries;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Features.UserManagement.Queries.MFA;

/// <summary>
/// Handler for GetMfaMethodsQuery
/// </summary>
public class GetMfaMethodsQueryHandler : IQueryHandler<GetMfaMethodsQuery, IEnumerable<MfaSetupDto>>
{
    private readonly IMfaMethodRepository _mfaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMfaMethodsQueryHandler> _logger;

    public GetMfaMethodsQueryHandler(
        IMfaMethodRepository mfaRepository,
        IMapper mapper,
        ILogger<GetMfaMethodsQueryHandler> logger)
    {
        _mfaRepository = mfaRepository ?? throw new ArgumentNullException(nameof(mfaRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<MfaSetupDto>> Handle(GetMfaMethodsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting MFA methods for user {UserId}, IsEnabled: {IsEnabled}", 
                request.UserId, request.IsEnabled);

            // Get MFA methods based on filter
            var mfaMethods = request.IsEnabled.HasValue
                ? await _mfaRepository.GetEnabledByUserIdAsync(request.UserId, cancellationToken)
                : await _mfaRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            // Map to DTOs
            var result = _mapper.Map<IEnumerable<MfaSetupDto>>(mfaMethods);

            _logger.LogInformation("Found {Count} MFA methods for user {UserId}", 
                result.Count(), request.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA methods for user {UserId}", request.UserId);
            throw;
        }
    }
} 