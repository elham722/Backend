using Backend.Application.Common.Results;
using MediatR;

namespace Backend.Application.Common.Commands
{
    /// <summary>
    /// Base interface for command handlers
    /// </summary>
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
    {
    }

    /// <summary>
    /// Base interface for command handlers that return data
    /// </summary>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    /// <typeparam name="TResponse">Type of the response data</typeparam>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
    }
} 