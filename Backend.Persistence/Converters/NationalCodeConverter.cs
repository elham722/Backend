using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Persistence.Converters;

/// <summary>
/// Value converter for NationalCode ValueObject
/// </summary>
public class NationalCodeConverter : ValueConverter<NationalCode?, string?>
{
    public NationalCodeConverter() : base(
        code => code == null ? null : code.Value,
        value => string.IsNullOrEmpty(value) ? null : NationalCode.Create(value))
    {
    }
} 