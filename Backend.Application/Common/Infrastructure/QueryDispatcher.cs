using MediatR;
using Backend.Application.Common.Interfaces.Infrastructure;

namespace Backend.Application.Common.Infrastructure
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IMediator _mediator;

        public QueryDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<TResult> DispatchAsync<TResult>(IRequest<TResult> query)
        {
            return await _mediator.Send(query);
        }
    }
} 