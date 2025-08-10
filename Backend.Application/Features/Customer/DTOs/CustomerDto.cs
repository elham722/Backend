using Backend.Application.Common.DTOs;

namespace Backend.Application.Features.Customer.DTOs;

/// <summary>
/// DTO for customer data
/// </summary>
public class CustomerDto : BaseDto
{
    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name (computed)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Age (computed)
    /// </summary>
    public int? Age => DateOfBirth?.Year > 0 ? DateTime.Now.Year - DateOfBirth.Value.Year : null;
    
    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Province
    /// </summary>
    public string? Province { get; set; }
    
    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// District
    /// </summary>
    public string? District { get; set; }
    
    /// <summary>
    /// Street
    /// </summary>
    public string? Street { get; set; }
    
    /// <summary>
    /// Postal Code
    /// </summary>
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Address Details
    /// </summary>
    public string? AddressDetails { get; set; }
    
    /// <summary>
    /// Full Address (computed)
    /// </summary>
    public string? FullAddress { get; set; }
    
    /// <summary>
    /// Customer status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Is customer verified
    /// </summary>
    public bool IsVerified { get; set; }
    
    /// <summary>
    /// Is customer premium
    /// </summary>
    public bool IsPremium { get; set; }
} 