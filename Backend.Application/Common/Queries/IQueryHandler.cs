using MediatR;

namespace Backend.Application.Common.Queries;

/// <summary>
/// Interface for query handlers
/// </summary>
/// <typeparam name="TQuery">Type of the query</typeparam>
/// <typeparam name="TResponse">Type of the response</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
} 