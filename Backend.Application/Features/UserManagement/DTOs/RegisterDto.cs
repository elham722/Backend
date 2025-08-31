using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Features.UserManagement.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "نام کاربری فقط می‌تواند شامل حروف، اعداد، خط تیره و زیرخط باشد")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "رمز عبور باید شامل حداقل یک حرف بزرگ، یک حرف کوچک، یک عدد و یک کاراکتر خاص باشد")]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
    [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "فرمت شماره تلفن صحیح نیست")]
    [Display(Name = "شماره تلفن")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "پذیرش قوانین الزامی است")]
    [Display(Name = "قوانین و شرایط را می‌پذیرم")]
    public bool AcceptTerms { get; set; } = false;
    
    [Display(Name = "اشتراک در خبرنامه")]
    public bool SubscribeToNewsletter { get; set; } = false;
    
    [Required(ErrorMessage = "تأیید CAPTCHA الزامی است")]
    [Display(Name = "تأیید امنیتی")]
    public string CaptchaToken { get; set; } = string.Empty;
    
    /// <summary>
    /// IP address of the registration attempt (for security tracking)
    /// </summary>
    [Display(Name = "آدرس IP")]
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Device information (browser, OS, device type)
    /// </summary>
    [Display(Name = "اطلاعات دستگاه")]
    public string? DeviceInfo { get; set; }
} 