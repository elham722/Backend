using AutoMapper;
using Backend.Application.Common.Extensions;
using Backend.Application.Common.Results;
using Backend.Application.Features.Customer.DTOs;
using Backend.Application.Features.Customer.Queries.GetAllCustomers;
using Backend.Application.Features.Customer.Specifications;
using Backend.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Backend.Application.Features.Customer.Queries.GetAllCustomers;

/// <summary>
/// Handler for getting all customers with pagination and filtering
/// </summary>
public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PaginatedResult<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCustomersQueryHandler> _logger;

    public GetAllCustomersQueryHandler(
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<GetAllCustomersQueryHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResult<CustomerDto>> Handle(
        GetAllCustomersQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting customers with pagination. Page: {PageNumber}, Size: {PageSize}", 
                request.PageNumber, request.PageSize);

            // Create specification with all filters
            var specification = new CustomerSpecification(
                searchTerm: request.SearchTerm,
                status: request.Status,
                customerType: request.CustomerType,
                isVerified: request.IsVerified,
                isPremium: request.IsPremium,
                createdFrom: request.CreatedFrom,
                createdTo: request.CreatedTo,
                minAge: request.MinAge,
                maxAge: request.MaxAge,
                country: request.Country,
                city: request.City,
                sortBy: request.SortBy,
                sortDirection: request.SortDirection,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            // Get queryable and apply specification
            var query = _customerRepository.GetQueryable();
            
            // Apply specification criteria
            query = query.Where(specification.Criteria);
            
            // Get total count
            var totalCount = await Task.FromResult(query.Count());
            
            // Apply sorting
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }
            
            // Apply pagination
            var paginatedCustomers = query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToList();

            // Map to DTOs
            var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(paginatedCustomers);

            _logger.LogInformation("Retrieved {Count} customers out of {TotalCount}", 
                paginatedCustomers.Count, totalCount);

            return PaginatedResult<CustomerDto>.Success(
                customerDtos, 
                totalCount, 
                request.PageNumber, 
                request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers with pagination");
            return PaginatedResult<CustomerDto>.Failure(ex);
        }
    }


} 