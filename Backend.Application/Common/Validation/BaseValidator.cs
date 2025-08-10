using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;

namespace Backend.Application.Common.Validation;

/// <summary>
/// Base validator class with common validation rules
/// </summary>
/// <typeparam name="T">Type to validate</typeparam>
public abstract class BaseValidator<T> : AbstractValidator<T>, IValidator<T>
{
    /// <summary>
    /// Validates the specified instance
    /// </summary>
    /// <param name="instance">Instance to validate</param>
    /// <returns>Validation result</returns>
    public virtual async Task<ValidationResult> ValidateAsync(T instance)
    {
        return await base.ValidateAsync(instance);
    }

    /// <summary>
    /// Adds common validation rules
    /// </summary>
    protected virtual void AddCommonRules()
    {
        // Override in derived classes to add common rules
    }

    /// <summary>
    /// Validates required string property
    /// </summary>
    /// <param name="expression">Property expression</param>
    /// <param name="maxLength">Maximum length</param>
    /// <param name="minLength">Minimum length</param>
    /// <returns>IRuleBuilder</returns>
    protected IRuleBuilder<T, string> ValidateRequiredString(
        Expression<Func<T, string>> expression, 
        int maxLength = 255, 
        int minLength = 1)
    {
        return RuleFor(expression)
            .NotEmpty()
            .WithMessage("{PropertyName} is required")
            .MaximumLength(maxLength)
            .WithMessage("{PropertyName} cannot exceed {MaxLength} characters")
            .MinimumLength(minLength)
            .WithMessage("{PropertyName} must be at least {MinLength} characters");
    }

    /// <summary>
    /// Validates email property
    /// </summary>
    /// <param name="expression">Property expression</param>
    /// <returns>IRuleBuilder</returns>
    protected IRuleBuilder<T, string> ValidateEmail(Expression<Func<T, string>> expression)
    {
        return RuleFor(expression)
            .NotEmpty()
            .WithMessage("{PropertyName} is required")
            .EmailAddress()
            .WithMessage("{PropertyName} must be a valid email address");
    }

    /// <summary>
    /// Validates phone number property
    /// </summary>
    /// <param name="expression">Property expression</param>
    /// <returns>IRuleBuilder</returns>
    protected IRuleBuilder<T, string> ValidatePhoneNumber(Expression<Func<T, string>> expression)
    {
        return RuleFor(expression)
            .NotEmpty()
            .WithMessage("{PropertyName} is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("{PropertyName} must be a valid phone number");
    }

    /// <summary>
    /// Validates required property of any type
    /// </summary>
    /// <typeparam name="TProperty">Property type</typeparam>
    /// <param name="expression">Property expression</param>
    /// <returns>IRuleBuilder</returns>
    protected IRuleBuilder<T, TProperty> ValidateRequired<TProperty>(
        Expression<Func<T, TProperty>> expression)
    {
        return RuleFor(expression)
            .NotEmpty()
            .WithMessage("{PropertyName} is required");
    }

    /// <summary>
    /// Validates nullable string property
    /// </summary>
    /// <param name="expression">Property expression</param>
    /// <param name="maxLength">Maximum length</param>
    /// <returns>IRuleBuilder</returns>
    protected IRuleBuilder<T, string?> ValidateOptionalString(
        Expression<Func<T, string?>> expression, 
        int maxLength = 255)
    {
        return RuleFor(expression)
            .MaximumLength(maxLength)
            .WithMessage("{PropertyName} cannot exceed {MaxLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(expression.Compile()(x)));
    }
} 