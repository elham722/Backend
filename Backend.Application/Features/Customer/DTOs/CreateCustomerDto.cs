namespace Backend.Application.Features.Customer.DTOs;

/// <summary>
/// DTO for creating a new customer
/// </summary>
public class CreateCustomerDto
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
} 