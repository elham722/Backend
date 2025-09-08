using Backend.Application.Common.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Identity.Services;

/// <summary>
/// Implementation of IUserService for user management operations
/// </summary>
public class ApplicationUserService : Backend.Application.Common.Interfaces.IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly ILogger<ApplicationUserService> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly BackendIdentityDbContext _context;

    public ApplicationUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IMapper mapper,
        ILogger<ApplicationUserService> logger,
        IJwtTokenService jwtTokenService,
        BackendIdentityDbContext context)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = ApplicationUser.Create(createUserDto.Email, createUserDto.UserName, createUserDto.CustomerId, createdBy, null);

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"Failed to create user: {errors}");
            }

            // Assign default role "User" to new users created by admin
            var roleResult = await _userManager.AddToRoleAsync(user, DatabaseSeeder.DefaultRoles.User);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign default role to user: {Email}. Errors: {Errors}", 
                    createUserDto.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                // Don't fail user creation if role assignment fails, just log it
            }
            else
            {
                _logger.LogInformation("Successfully assigned default role 'User' to new user: {Email}", createUserDto.Email);
            }

            var userDto = _mapper.Map<UserDto>((IApplicationUser)user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", createUserDto.Email);
            return Result<UserDto>.Failure("An error occurred while creating the user");
        }
    }

    public async Task<Result<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Update basic properties
            user.Email = updateUserDto.Email ?? user.Email;
            user.PhoneNumber = updateUserDto.PhoneNumber ?? user.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"Failed to update user: {errors}");
            }

            var userDto = _mapper.Map<UserDto>((IApplicationUser)user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while updating the user");
        }
    }

    public async Task<Result> DeleteUserAsync(string userId, string deletedBy, bool permanentDelete = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            if (permanentDelete)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to delete user: {errors}");
                }
            }
            else
            {
                // Soft delete by updating AccountInfo
                var accountInfo = user.Account;
                accountInfo = accountInfo.MarkAsDeleted(new DefaultDateTimeService());
                user.UpdateAccount(accountInfo);
                
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to deactivate user: {errors}");
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return Result.Failure("An error occurred while deleting the user");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(string userId, bool includeRoles = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.Map<UserDto>((IApplicationUser)user);
            
            if (includeRoles)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Roles = roles.ToList();
            }

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while getting the user");
        }
    }

    public async Task<Result<PaginationResponse<UserDto>>> GetUsersAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null,
        string? status = null,
        string? role = null,
        bool? emailConfirmed = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var users = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.Email.Contains(searchTerm) || u.UserName.Contains(searchTerm));
            }

            if (emailConfirmed.HasValue)
            {
                users = users.Where(u => u.EmailConfirmed == emailConfirmed.Value);
            }

            if (isActive.HasValue)
            {
                users = users.Where(u => u.IsActive == isActive.Value);
            }

            if (!includeDeleted)
            {
                users = users.Where(u => u.IsActive);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isDescending = sortDirection?.ToLower() == "desc";
                users = sortBy.ToLower() switch
                {
                    "email" => isDescending ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
                    "username" => isDescending ? users.OrderByDescending(u => u.UserName) : users.OrderBy(u => u.UserName),
                    _ => users.OrderBy(u => u.Email)
                };
            }
            else
            {
                users = users.OrderBy(u => u.Email);
            }

            // Apply pagination
            var totalCount = users.Count();
            var pagedUsers = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var userDtos = _mapper.Map<List<UserDto>>(pagedUsers);

            // Load roles for each user
            foreach (var userDto in userDtos)
            {
                var user = pagedUsers.FirstOrDefault(u => u.Id == userDto.Id);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                }
            }

            var paginationResponse = new PaginationResponse<UserDto>
            {
                Data = userDtos,
                Meta = new PaginationMetaData
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                },
                Links = new PaginationLinks()
            };

            return Result<PaginationResponse<UserDto>>.Success(paginationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return Result<PaginationResponse<UserDto>>.Failure("An error occurred while getting users");
        }
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest loginDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername) ?? 
                      await _userManager.FindByNameAsync(loginDto.EmailOrUsername);
            
            if (user == null)
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                return Result<LoginResponse>.Failure("Invalid email or password");
            }

            // Generate JWT access token
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync((IApplicationUser)user, cancellationToken);
            
            // Generate refresh token
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();
            
            // Store refresh token in database
            var userToken = new Microsoft.AspNetCore.Identity.IdentityUserToken<string>
            {
                UserId = user.Id,
                LoginProvider = "Backend",
                Name = "RefreshToken",
                Value = refreshToken
            };
            
            // Remove any existing refresh tokens for this user
            var existingTokens = await _context.UserTokens
                .Where(ut => ut.UserId == user.Id && ut.Name == "RefreshToken")
                .ToListAsync(cancellationToken);
            
            foreach (var existingToken in existingTokens)
            {
                _context.UserTokens.Remove(existingToken);
            }
            
            // Add new refresh token
            await _context.UserTokens.AddAsync(userToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Parse JWT token to get expiration time
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);
            var expiresAt = jwtToken.ValidTo;

            var loginResponse = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>((IApplicationUser)user)
            };

            var finalResult = Result<LoginResponse>.Success(loginResponse);
            
            // DEBUG: Test Result creation
            _logger.LogInformation("DEBUG RESULT: IsSuccess={isSuccess}, Data={data}, StatusCode={statusCode}", 
                finalResult.IsSuccess, 
                finalResult.Data != null ? "Not null" : "NULL",
                finalResult.StatusCode);
            
            _logger.LogInformation("LoginAsync final result - IsSuccess: {IsSuccess}, Data: {Data}, AccessToken: {AccessToken}",
                finalResult.IsSuccess, 
                finalResult.Data != null ? "Not null" : "NULL",
                finalResult.Data?.AccessToken != null ? "Generated" : "NULL");
            
            return finalResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login: {Email}", loginDto.EmailOrUsername);
            return Result<LoginResponse>.Failure("An error occurred during login");
        }
    }

    public async Task<Result<LoginResponse>> RegisterAsync(RegisterDto registerDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return Result<LoginResponse>.Failure("User with this email already exists");
            }

            existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUser != null)
            {
                return Result<LoginResponse>.Failure("User with this username already exists");
            }

            // Create new user
            var user = ApplicationUser.Create(registerDto.Email, registerDto.UserName, null, "System", null);

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {Email}. Errors: {Errors}", registerDto.Email, errors);
                return Result<LoginResponse>.Failure($"Failed to create user: {errors}");
            }

            _logger.LogInformation("User created successfully: {Email}", registerDto.Email);

            // Assign default role "User" to new users
            var roleResult = await _userManager.AddToRoleAsync(user, DatabaseSeeder.DefaultRoles.User);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign default role to user: {Email}. Errors: {Errors}", 
                    registerDto.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                // Don't fail registration if role assignment fails, just log it
            }
            else
            {
                _logger.LogInformation("Successfully assigned default role '{Role}' to new user: {Email}", 
                    DatabaseSeeder.DefaultRoles.User, registerDto.Email);
            }

            // Generate JWT access token
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync((IApplicationUser)user, cancellationToken);
            
            // Generate refresh token
            var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();
            
            // Store refresh token in database
            var userToken = new Microsoft.AspNetCore.Identity.IdentityUserToken<string>
            {
                UserId = user.Id,
                LoginProvider = "Backend",
                Name = "RefreshToken",
                Value = refreshToken
            };
            
            // Add new refresh token
            await _context.UserTokens.AddAsync(userToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Parse JWT token to get expiration time
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);
            var expiresAt = jwtToken.ValidTo;

            var loginResponse = new LoginResponse
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>((IApplicationUser)user)
            };

            _logger.LogInformation("Registration completed successfully for user: {Email}", registerDto.Email);
            return Result<LoginResponse>.Success(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration: {Email}", registerDto.Email);
            return Result<LoginResponse>.Failure("An error occurred during registration");
        }
    }

    public async Task<Result> ChangePasswordAsync(string? userId, string? currentPassword, string newPassword, string changedBy, bool requirePasswordChangeOnNextLogin = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Result.Failure("User ID is required");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            IdentityResult result;
            if (!string.IsNullOrEmpty(currentPassword))
            {
                result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to change password: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return Result.Failure("An error occurred while changing the password");
        }
    }

    public async Task<Result> ActivateUserAsync(string userId, string activatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Update AccountInfo to activate
            var accountInfo = user.Account;
            accountInfo = accountInfo.Activate();
            user.UpdateAccount(accountInfo);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to activate user: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user: {UserId}", userId);
            return Result.Failure("An error occurred while activating the user");
        }
    }

    public async Task<Result> DeactivateUserAsync(string userId, string deactivatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            // Update AccountInfo to deactivate
            var accountInfo = user.Account;
            accountInfo = accountInfo.Deactivate();
            user.UpdateAccount(accountInfo);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to deactivate user: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return Result.Failure("An error occurred while deactivating the user");
        }
    }

    public async Task<Result> ResetPasswordAsync(string userId, string newPassword, string resetBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to reset password: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
            return Result.Failure("An error occurred while resetting the password");
        }
    }

    public async Task<Result> AssignRolesAsync(string userId, List<string> roles, string assignedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var result = await _userManager.AddToRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to assign roles: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user: {UserId}", userId);
            return Result.Failure("An error occurred while assigning roles");
        }
    }

    public async Task<Result> RemoveRolesAsync(string userId, List<string> roles, string removedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to remove roles: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing roles from user: {UserId}", userId);
            return Result.Failure("An error occurred while removing roles");
        }
    }

    public async Task<Result<List<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<List<string>>.Failure("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Result<List<string>>.Success(roles.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
            return Result<List<string>>.Failure("An error occurred while getting user roles");
        }
    }

    public async Task<Result<bool>> UserHasRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            var hasRole = await _userManager.IsInRoleAsync(user, role);
            return Result<bool>.Success(hasRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role for user: {UserId}", userId);
            return Result<bool>.Failure("An error occurred while checking user role");
        }
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to confirm email: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
            return Result.Failure("An error occurred while confirming email");
        }
    }

    public async Task<Result> SendEmailConfirmationAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // TODO: Implement email sending
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation for user: {UserId}", userId);
            return Result.Failure("An error occurred while sending email confirmation");
        }
    }

    public async Task<Result> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: Implement email sending
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email: {Email}", email);
            return Result.Failure("An error occurred while sending password reset email");
        }
    }

    public async Task<Result> ResetPasswordWithTokenAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to reset password: {errors}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token: {Email}", email);
            return Result.Failure("An error occurred while resetting password");
        }
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get principal from expired token
            var principal = await _jwtTokenService.GetPrincipalFromExpiredTokenAsync(refreshTokenDto.AccessToken);
            if (principal == null)
            {
                return Result<LoginResponse>.Failure("Invalid access token");
            }

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Result<LoginResponse>.Failure("Invalid token claims");
            }

            // Validate refresh token
            var isValidRefreshToken = await _jwtTokenService.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken, cancellationToken);
            if (!isValidRefreshToken)
            {
                return Result<LoginResponse>.Failure("Invalid refresh token");
            }

            // Get user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<LoginResponse>.Failure("User not found");
            }

            // Generate new access token
            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync((IApplicationUser)user, cancellationToken);
            
            // Generate new refresh token
            var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync();
            
            // Revoke old refresh token
            await _jwtTokenService.RevokeRefreshTokenAsync(userId, refreshTokenDto.RefreshToken, "Token refresh", "Token rotation", cancellationToken);
            
            // Store new refresh token
            var userToken = new Microsoft.AspNetCore.Identity.IdentityUserToken<string>
            {
                UserId = user.Id,
                LoginProvider = "Backend",
                Name = "RefreshToken",
                Value = newRefreshToken
            };
            
            await _context.UserTokens.AddAsync(userToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Parse JWT token to get expiration time
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(newAccessToken);
            var expiresAt = jwtToken.ValidTo;

            var loginResponse = new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt,
                User = _mapper.Map<UserDto>((IApplicationUser)user)
            };

            return Result<LoginResponse>.Success(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Result<LoginResponse>.Failure("An error occurred while refreshing token");
        }
    }

    public async Task<Result<LogoutResultDto>> LogoutAsync(LogoutDto logoutDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get principal from access token if available
            string? userId = null;
            if (!string.IsNullOrEmpty(logoutDto.RefreshToken))
            {
                // Find user by refresh token
                var userToken = await _context.UserTokens
                    .FirstOrDefaultAsync(ut => ut.Name == "RefreshToken" && ut.Value == logoutDto.RefreshToken, cancellationToken);
                
                if (userToken != null)
                {
                    userId = userToken.UserId;
                }
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Result<LogoutResultDto>.Failure("Invalid logout request");
            }

            // Revoke refresh token
            if (!string.IsNullOrEmpty(logoutDto.RefreshToken))
            {
                await _jwtTokenService.RevokeRefreshTokenAsync(userId, logoutDto.RefreshToken, userId, "User logout", cancellationToken);
            }

            // If logout from all devices, revoke all refresh tokens
            if (logoutDto.LogoutFromAllDevices)
            {
                await _jwtTokenService.RevokeAllRefreshTokensAsync(userId, userId, "Logout from all devices", cancellationToken);
            }

            var logoutResult = new LogoutResultDto
            {
                IsSuccess = true,
                ErrorMessage = null
            };

            return Result<LogoutResultDto>.Success(logoutResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Result<LogoutResultDto>.Failure("An error occurred during logout");
        }
    }
}