using MediatR;

namespace Backend.Application.Common.Interfaces.Infrastructure
{
    public interface ICommandDispatcher
    {
        Task<TResult> DispatchAsync<TResult>(IRequest<TResult> command);
        Task DispatchAsync(IRequest command);
    }
} 