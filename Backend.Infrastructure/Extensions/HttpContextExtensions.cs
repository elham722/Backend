using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Interfaces.Infrastructure;

namespace Backend.Infrastructure.Extensions
{
    public static class HttpContextExtensions
    {
        public static IQueryDispatcher QueryDispatcher(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<IQueryDispatcher>();
        }

        public static ICommandDispatcher CommandDispatcher(this HttpContext context)
        {
            return context.RequestServices.GetRequiredService<ICommandDispatcher>();
        }
    }
} 