using FluentValidation;
using MediatR;
using Backend.Application.Common.Results;
using Backend.Application.Common.Validation;

namespace Backend.Application.Common.Behaviors;

/// <summary>
/// Behavior for validating requests before processing
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<FluentValidation.IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<FluentValidation.IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) 
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var errorCode = "VALIDATION_ERROR";
            
            // Create appropriate failure result based on response type
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errorMessage, errorCode);
            }
            else if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result<>).MakeGenericType(dataType).GetMethod("Failure", new[] { typeof(string), typeof(string) });
                return (TResponse)failureMethod!.Invoke(null, new object[] { errorMessage, errorCode })!;
            }
        }

        return await next();
    }
} 