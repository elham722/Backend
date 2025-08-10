using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Common.Commands;

/// <summary>
/// Base interface for all commands
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Base interface for commands that return data
/// </summary>
/// <typeparam name="TResponse">Type of the response data</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
} 