using MediatR;

namespace Backend.Application.Common.Queries;

/// <summary>
/// Base interface for all queries
/// </summary>
/// <typeparam name="TResponse">Type of the response data</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
} 