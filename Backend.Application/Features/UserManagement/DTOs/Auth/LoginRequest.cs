using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Features.UserManagement.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "ایمیل یا نام کاربری الزامی است")]
    [Display(Name = "ایمیل یا نام کاربری")]
    public string EmailOrUsername { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; } = false;
  
    [Display(Name = "کد تایید دو مرحله‌ای")]
    public string? TwoFactorCode { get; set; }
    
    /// <summary>
    /// IP address of the login attempt (for security tracking)
    /// </summary>
    [Display(Name = "آدرس IP")]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Device information (browser, OS, device type)
    /// </summary>
    [Display(Name = "اطلاعات دستگاه")]
    public string? DeviceInfo { get; set; }
} 