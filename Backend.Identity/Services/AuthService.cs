using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of authentication service using ASP.NET Core Identity
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(string userName, string password, bool rememberMe)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {UserName}", userName);

                // Find user by username
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found - {UserName}", userName);
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "Invalid username or password",
                        Errors = new List<string> { "Invalid credentials" }
                    };
                }

                // Check if user can login
                if (!user.CanLogin())
                {
                    _logger.LogWarning("Login failed: User cannot login - {UserName}, IsActive: {IsActive}, IsLocked: {IsLocked}", 
                        userName, user.IsActive, user.IsLocked);
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "Account is not active or is locked",
                        Errors = new List<string> { "Account inactive" }
                    };
                }

                // Attempt to sign in
                var signInResult = await _signInManager.PasswordSignInAsync(
                    user, 
                    password, 
                    rememberMe, 
                    lockoutOnFailure: true);

                if (signInResult.Succeeded)
                {
                    // Update last login
                    user.UpdateLastLogin();
                    await _userManager.UpdateAsync(user);

                    // Get user roles
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                    var refreshToken = _jwtService.GenerateRefreshToken();
                    var expiresAt = _jwtService.GetTokenExpiration(token);

                    _logger.LogInformation("Login successful for user: {UserName}", userName);

                    return new AuthResult
                    {
                        IsSuccess = true,
                        Message = "Login successful",
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Token = token,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiresAt,
                        Roles = roles.ToList()
                    };
                }

                if (signInResult.IsLockedOut)
                {
                    _logger.LogWarning("Login failed: Account locked out - {UserName}", userName);
                    return new AuthResult
                    {
                        IsSuccess = false,
                        IsLockedOut = true,
                        Message = "Account is locked out",
                        Errors = new List<string> { "Account locked" }
                    };
                }

                if (signInResult.RequiresTwoFactor)
                {
                    _logger.LogInformation("Login requires 2FA for user: {UserName}", userName);
                    return new AuthResult
                    {
                        IsSuccess = false,
                        RequiresTwoFactor = true,
                        Message = "Two-factor authentication required",
                        Errors = new List<string> { "2FA required" }
                    };
                }

                // Increment login attempts
                user.IncrementLoginAttempts();
                await _userManager.UpdateAsync(user);

                _logger.LogWarning("Login failed: Invalid password for user: {UserName}", userName);
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "Invalid username or password",
                    Errors = new List<string> { "Invalid credentials" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user: {UserName}", userName);
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { "Login error" }
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(string userName, string email, string password, string? phoneNumber)
        {
            try
            {
                _logger.LogInformation("Registration attempt for user: {UserName}, Email: {Email}", userName, email);

                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Username already exists - {UserName}", userName);
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "Username already exists",
                        Errors = new List<string> { "Username exists" }
                    };
                }

                // Check if email already exists
                existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: Email already exists - {Email}", email);
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "Email already exists",
                        Errors = new List<string> { "Email exists" }
                    };
                }

                // Create new user
                var user = ApplicationUser.Create(email, userName);
                user.PhoneNumber = phoneNumber;

                // Create user with password
                var result = await _userManager.CreateAsync(user, password);
                
                if (result.Succeeded)
                {
                    // Get user roles (default to empty list for new users)
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                    var refreshToken = _jwtService.GenerateRefreshToken();
                    var expiresAt = _jwtService.GetTokenExpiration(token);

                    _logger.LogInformation("Registration successful for user: {UserName}", userName);

                    return new AuthResult
                    {
                        IsSuccess = true,
                        Message = "User registered successfully",
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Token = token,
                        RefreshToken = refreshToken,
                        ExpiresAt = expiresAt,
                        Roles = roles.ToList()
                    };
                }
                
                // Registration failed
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Registration failed for user: {UserName}, Errors: {Errors}", 
                    userName, string.Join(", ", errors));
                
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "Registration failed",
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for user: {UserName}", userName);
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "An error occurred during registration",
                    Errors = new List<string> { "Registration error" }
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return false;
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string userId)
        {
            try
            {
                // Validate refresh token
                if (!_jwtService.ValidateRefreshToken(refreshToken))
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "Invalid refresh token",
                        Errors = new List<string> { "Invalid refresh token" }
                    };
                }

                // Get user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        Errors = new List<string> { "User not found" }
                    };
                }

                // Generate new tokens
                var roles = await _userManager.GetRolesAsync(user);
                var newToken = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var expiresAt = _jwtService.GetTokenExpiration(newToken);

                return new AuthResult
                {
                    IsSuccess = true,
                    Message = "Token refreshed successfully",
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = expiresAt,
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh for user: {UserId}", userId);
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "An error occurred during token refresh",
                    Errors = new List<string> { "Token refresh error" }
                };
            }
        }

        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                return new UserProfile
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.Account.CreatedAt,
                    LastLoginAt = user.Account.LastLoginAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user profile for user: {UserId}", userId);
                return null;
            }
        }
    }
} 