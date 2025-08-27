using Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Persistence.Converters;

/// <summary>
/// Value converter for Email ValueObject
/// </summary>
public class EmailConverter : ValueConverter<Email?, string?>
{
    public EmailConverter() : base(
        email => email == null ? null : email.ToString(),
        value => string.IsNullOrEmpty(value) ? null : Email.Create(value))
    {
    }
} 