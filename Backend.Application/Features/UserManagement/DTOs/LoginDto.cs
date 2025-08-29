using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Features.UserManagement.DTOs;

public class LoginDto
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
} 