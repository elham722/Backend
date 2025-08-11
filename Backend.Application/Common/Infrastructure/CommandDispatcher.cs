using MediatR;
using Backend.Application.Common.Interfaces.Infrastructure;

namespace Backend.Application.Common.Infrastructure
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IMediator _mediator;

        public CommandDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<TResult> DispatchAsync<TResult>(IRequest<TResult> command)
        {
            return await _mediator.Send(command);
        }

        public async Task DispatchAsync(IRequest command)
        {
            await _mediator.Send(command);
        }
    }
} 