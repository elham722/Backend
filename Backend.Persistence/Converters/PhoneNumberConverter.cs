using Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Persistence.Converters;

/// <summary>
/// Value converter for PhoneNumber ValueObject
/// </summary>
public class PhoneNumberConverter : ValueConverter<PhoneNumber?, string?>
{
    public PhoneNumberConverter() : base(
        phone => phone == null ? null : phone.ToString(),
        value => string.IsNullOrEmpty(value) ? null : PhoneNumber.Create(value))
    {
    }
} 