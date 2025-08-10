using FluentValidation.Results;

namespace Backend.Application.Common.Validation;

/// <summary>
/// Base interface for all validators
/// </summary>
/// <typeparam name="T">Type to validate</typeparam>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the specified instance
    /// </summary>
    /// <param name="instance">Instance to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAsync(T instance);
} 