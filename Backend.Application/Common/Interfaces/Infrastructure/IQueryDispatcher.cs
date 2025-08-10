using MediatR;

namespace Backend.Application.Common.Interfaces.Infrastructure
{
    public interface IQueryDispatcher
    {
        Task<TResult> DispatchAsync<TResult>(IRequest<TResult> query);
    }
} 