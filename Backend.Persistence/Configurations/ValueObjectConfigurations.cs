using System.Linq.Expressions;
using Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Configurations;

/// <summary>
/// Configuration for Value Objects to work properly with Entity Framework
/// </summary>
public static class ValueObjectConfigurations
{
    /// <summary>
    /// Configure Email value object conversion
    /// </summary>
    public static void ConfigureEmail<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, Email>> propertyExpression)
        where T : class
    {
        builder.Property(propertyExpression)
            .HasConversion(
                email => email.ToString(),
                value => Email.Create(value))
            .HasMaxLength(255);
    }

    /// <summary>
    /// Configure PhoneNumber value object conversion
    /// </summary>
    public static void ConfigurePhoneNumber<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, PhoneNumber>> propertyExpression)
        where T : class
    {
        builder.Property(propertyExpression)
            .HasConversion(
                phone => phone.ToString(),
                value => PhoneNumber.Create(value))
            .HasMaxLength(20);
    }
} 