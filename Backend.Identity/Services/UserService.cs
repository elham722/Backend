using Backend.Application.Common.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Common.Interfaces;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Identity.Models;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Linq;

namespace Backend.Identity.Services;

/// <summary>
/// Implementation of IUserService for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserMapper _userMapper;
    private readonly IAccountManagementService _accountManagementService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IUserMapper userMapper,
        IAccountManagementService accountManagementService,
        IDateTimeService dateTimeService,
        ILogger<UserService> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _userMapper = userMapper ?? throw new ArgumentNullException(nameof(userMapper));
        _accountManagementService = accountManagementService ?? throw new ArgumentNullException(nameof(accountManagementService));
        _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("User with this email already exists", "UserAlreadyExists");
            }

            existingUser = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("Username is already taken", "UsernameAlreadyTaken");
            }

            // Create new user
            var user = ApplicationUser.Create(
                createUserDto.Email,
                createUserDto.UserName,
                createUserDto.CustomerId,
                createdBy,
                _dateTimeService);

            if (!string.IsNullOrEmpty(createUserDto.PhoneNumber))
            {
                user.PhoneNumber = createUserDto.PhoneNumber;
            }

            // Create user with password using helper method
            var createResult = await CreateUserWithPasswordAsync(user, createUserDto.Password);
            if (!createResult.IsSuccess)
            {
                return Result<UserDto>.Failure(createResult.ErrorMessage, createResult.ErrorCode);
            }
            
            user = createResult.Data;

            // Assign roles if specified
            if (createUserDto.Roles?.Any() == true)
            {
                var roleResult = await AssignRolesAsync(user.Id, createUserDto.Roles, createdBy, cancellationToken);
                if (!roleResult.IsSuccess)
                {
                    _logger.LogWarning("User created but role assignment failed: {UserId}, Error: {Error}", user.Id, roleResult.ErrorMessage);
                }
            }

            // Set password change requirement if specified
            // Note: This would need to be implemented in the SecurityInfo value object
            // For now, we'll handle this through the AccountInfo or a separate mechanism
            if (createUserDto.RequirePasswordChange)
            {
                // TODO: Implement password change requirement logic
                _logger.LogInformation("Password change requirement set for user: {UserId}", user.Id);
            }

            // Send confirmation email if requested
            if (createUserDto.SendConfirmationEmail)
            {
                await SendEmailConfirmationAsync(user.Id, cancellationToken);
            }

            // Get user with roles for response
            var userWithRoles = await GetUserWithRolesAsync(user.Id);
            var userDto = _userMapper.MapToUserDto(userWithRoles, userWithRoles.Roles);

            _logger.LogInformation("User created successfully: {UserId}, {Email}", user.Id, user.Email);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", createUserDto.Email);
            return Result<UserDto>.Failure("An error occurred while creating the user", "UserCreationError");
        }
    }

    public async Task<Result<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto, string updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found", "UserNotFound");
            }

            // Update basic properties
            if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, updateUserDto.Email);
                if (!emailResult.Succeeded)
                {
                    var errors = string.Join(", ", emailResult.Errors.Select(e => e.Description));
                    return Result<UserDto>.Failure($"Failed to update email: {errors}", "EmailUpdateFailed");
                }
            }

            if (!string.IsNullOrEmpty(updateUserDto.UserName) && updateUserDto.UserName != user.UserName)
            {
                var usernameResult = await _userManager.SetUserNameAsync(user, updateUserDto.UserName);
                if (!usernameResult.Succeeded)
                {
                    var errors = string.Join(", ", usernameResult.Errors.Select(e => e.Description));
                    return Result<UserDto>.Failure($"Failed to update username: {errors}", "UsernameUpdateFailed");
                }
            }

            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
            {
                user.PhoneNumber = updateUserDto.PhoneNumber;
            }

            // Update audit info
            var updatedAudit = user.Audit.Update(updatedBy, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result<UserDto>.Failure($"Failed to update user: {errors}", "UserUpdateFailed");
            }

            // Get updated user with roles
            var updatedUser = await GetUserWithRolesAsync(user.Id);
            var userDto = _userMapper.MapToUserDto(updatedUser, updatedUser.Roles);

            _logger.LogInformation("User updated successfully: {UserId}", userId);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while updating the user", "UserUpdateError");
        }
    }

    public async Task<Result> DeleteUserAsync(string userId, string deletedBy, bool permanentDelete = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            if (permanentDelete)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to delete user: {errors}", "UserDeletionFailed");
                }
            }
            else
            {
                // Soft delete
                await _accountManagementService.MarkAsDeletedAsync(user);
            }

            _logger.LogInformation("User deleted successfully: {UserId}, Permanent: {Permanent}", userId, permanentDelete);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return Result.Failure("An error occurred while deleting the user", "UserDeletionError");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(string userId, bool includeRoles = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found", "UserNotFound");
            }

            if (includeRoles)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _userMapper.MapToUserDto(user, roles.ToList());
                return Result<UserDto>.Success(userDto);
            }

            var userDtoWithoutRoles = _userMapper.MapToUserDto(user);
            return Result<UserDto>.Success(userDtoWithoutRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return Result<UserDto>.Failure("An error occurred while retrieving the user", "UserRetrievalError");
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
            var query = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => 
                    u.Email.Contains(searchTerm) || 
                    u.UserName.Contains(searchTerm) || 
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "active":
                        query = query.Where(u => u.IsActive);
                        break;
                    case "inactive":
                        query = query.Where(u => !u.IsActive);
                        break;
                    case "locked":
                        query = query.Where(u => u.IsLocked);
                        break;
                }
            }

            if (emailConfirmed.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == emailConfirmed.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!includeDeleted)
            {
                query = query.Where(u => !u.Account.IsDeleted);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "email" => sortDirection?.ToLower() == "desc" 
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email),
                    "username" => sortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.UserName)
                        : query.OrderBy(u => u.UserName),
                    "created" => sortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.Account.CreatedAt)
                        : query.OrderBy(u => u.Account.CreatedAt),
                    _ => query.OrderBy(u => u.UserName)
                };
            }
            else
            {
                query = query.OrderBy(u => u.UserName);
            }

            // Get total count
            var totalCount = await Task.FromResult(query.Count());

            // Apply pagination
            var users = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get roles for all users
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _userMapper.MapToUserDto(user, roles.ToList());
                userDtos.Add(userDto);
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var hasPreviousPage = pageNumber > 1;
            var hasNextPage = pageNumber < totalPages;

            var paginationResponse = new PaginationResponse<UserDto>
            {
                Data = userDtos,
                Meta = new PaginationMetaData
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = hasPreviousPage,
                    HasNextPage = hasNextPage
                },
                Links = new PaginationLinks
                {
                    First = $"?pageNumber=1&pageSize={pageSize}",
                    Last = $"?pageNumber={totalPages}&pageSize={pageSize}",
                    Previous = hasPreviousPage ? $"?pageNumber={pageNumber - 1}&pageSize={pageSize}" : null,
                    Next = hasNextPage ? $"?pageNumber={pageNumber + 1}&pageSize={pageSize}" : null
                }
            };

            return Result<PaginationResponse<UserDto>>.Success(paginationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with pagination");
            return Result<PaginationResponse<UserDto>>.Failure("An error occurred while retrieving users", "UsersRetrievalError");
        }
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto loginDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to find user by email first, then by username
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(loginDto.EmailOrUsername);
            }
            if (user == null)
            {
                return Result<AuthResultDto>.Failure("Invalid email or password", "InvalidCredentials");
            }

            // Check if user can login
            if (!user.CanLogin())
            {
                if (user.IsAccountLocked)
                {
                    return Result<AuthResultDto>.Failure("Account is locked", "AccountLocked");
                }
                if (!user.IsActive)
                {
                    return Result<AuthResultDto>.Failure("Account is deactivated", "AccountDeactivated");
                }
                return Result<AuthResultDto>.Failure("Account is not accessible", "AccountInaccessible");
            }

            // Attempt sign in
            var signInResult = await _signInManager.PasswordSignInAsync(
                user, 
                loginDto.Password, 
                loginDto.RememberMe, 
                lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                // Update last login
                await _accountManagementService.UpdateLastLoginAsync(user);

                // Reset login attempts
                if (user.AccessFailedCount > 0)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _userMapper.MapToUserDto(user, roles.ToList());

                // Generate JWT tokens
                var claims = await _accountManagementService.GetUserClaimsAsync(user);
                var accessToken = _accountManagementService.GenerateAccessToken(claims);
                var refreshToken = _accountManagementService.GenerateRefreshToken();

                var authResult = new AuthResultDto
                {
                    IsSuccess = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Configure from settings
                    User = userDto,
                    RequiresEmailConfirmation = !user.EmailConfirmed,
                    RequiresPasswordChange = user.RequiresPasswordChange()
                };

                _logger.LogInformation("User logged in successfully: {UserId}, {Email}", user.Id, user.Email);
                return Result<AuthResultDto>.Success(authResult);
            }
            else if (signInResult.IsLockedOut)
            {
                await _accountManagementService.IncrementLoginAttemptsAsync(user);
                return Result<AuthResultDto>.Failure("Account is temporarily locked due to multiple failed login attempts", "AccountLockedOut");
            }
            else if (signInResult.RequiresTwoFactor)
            {
                var authResult = new AuthResultDto
                {
                    IsSuccess = false,
                    RequiresTwoFactor = true,
                    ErrorMessage = "Two-factor authentication is required"
                };
                return Result<AuthResultDto>.Success(authResult);
            }
            else
            {
                await _accountManagementService.IncrementLoginAttemptsAsync(user);
                return Result<AuthResultDto>.Failure("Invalid email or password", "InvalidCredentials");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", loginDto.EmailOrUsername);
            return Result<AuthResultDto>.Failure("An error occurred during login", "LoginError");
        }
    }

    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto registerDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return Result<AuthResultDto>.Failure("User with this email already exists", "UserAlreadyExists");
            }

            existingUser = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUser != null)
            {
                return Result<AuthResultDto>.Failure("Username is already taken", "UsernameAlreadyTaken");
            }

            // Create user
            var user = ApplicationUser.Create(
                registerDto.Email,
                registerDto.UserName,
                null, // No customer ID for registration
                registerDto.UserName, // Created by self
                _dateTimeService);

            if (!string.IsNullOrEmpty(registerDto.PhoneNumber))
            {
                user.PhoneNumber = registerDto.PhoneNumber;
            }

            // Create user with password using helper method
            var createResult = await CreateUserWithPasswordAsync(user, registerDto.Password);
            if (!createResult.IsSuccess)
            {
                return Result<AuthResultDto>.Failure(createResult.ErrorMessage, createResult.ErrorCode);
            }
            
            user = createResult.Data;

            // Assign default role (you can customize this based on your requirements)
            // For now, we'll assign a default "User" role if it exists
            var defaultRole = "User";
            if (await _roleManager.RoleExistsAsync(defaultRole))
            {
                var roleResult = await AssignRolesAsync(user.Id, new List<string> { defaultRole }, user.UserName, cancellationToken);
                if (!roleResult.IsSuccess)
                {
                    _logger.LogWarning("User registered but role assignment failed: {UserId}, Error: {Error}", user.Id, roleResult.ErrorMessage);
                }
            }

            // Send confirmation email
            await SendEmailConfirmationAsync(user.Id, cancellationToken);

            var userDto = _userMapper.MapToUserDto(user);
            var authResult = new AuthResultDto
            {
                IsSuccess = true,
                User = userDto,
                RequiresEmailConfirmation = true,
                ErrorMessage = "Registration successful. Please check your email to confirm your account."
            };

            _logger.LogInformation("User registered successfully: {UserId}, {Email}", user.Id, user.Email);
            return Result<AuthResultDto>.Success(authResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
            return Result<AuthResultDto>.Failure("An error occurred during registration", "RegistrationError");
        }
    }

    public async Task<Result> ChangePasswordAsync(string? userId, string? currentPassword, string newPassword, string changedBy, bool requirePasswordChangeOnNextLogin = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Result.Failure("User ID is required", "UserIdRequired");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            // Change password using helper method
            var changeResult = await ChangeUserPasswordAsync(user, newPassword, currentPassword);
            if (!changeResult.IsSuccess)
            {
                return changeResult;
            }

            // Update password change timestamp
            await _accountManagementService.UpdatePasswordChangeAsync(user);

            // Set password change requirement if specified
            // Note: This would need to be implemented in the SecurityInfo value object
            if (requirePasswordChangeOnNextLogin)
            {
                // TODO: Implement password change requirement logic
                _logger.LogInformation("Password change requirement set for user: {UserId}", userId);
            }

            // Update audit info
            var updatedAudit = user.Audit.Update(changedBy, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return Result.Failure("An error occurred while changing the password", "PasswordChangeError");
        }
    }

    public async Task<Result> ActivateUserAsync(string userId, string activatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            if (user.IsActive)
            {
                return Result.Failure("User is already active", "UserAlreadyActive");
            }

            await _accountManagementService.ActivateAccountAsync(user);

            _logger.LogInformation("User activated successfully: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user: {UserId}", userId);
            return Result.Failure("An error occurred while activating the user", "UserActivationError");
        }
    }

    public async Task<Result> DeactivateUserAsync(string userId, string deactivatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            if (!user.IsActive)
            {
                return Result.Failure("User is already deactivated", "UserAlreadyDeactivated");
            }

            await _accountManagementService.DeactivateAccountAsync(user);

            _logger.LogInformation("User deactivated successfully: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return Result.Failure("An error occurred while deactivating the user", "UserDeactivationError");
        }
    }

    public async Task<Result> ResetPasswordAsync(string userId, string newPassword, string resetBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Reset password using token
            var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to reset password: {errors}", "PasswordResetFailed");
            }

            // Update password change timestamp
            await _accountManagementService.UpdatePasswordChangeAsync(user);

            // Update audit info
            var updatedAudit = user.Audit.Update(resetBy, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password reset successfully for user: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
            return Result.Failure("An error occurred while resetting the password", "PasswordResetError");
        }
    }

    public async Task<Result> AssignRolesAsync(string userId, List<string> roles, string assignedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            // Validate roles exist
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return Result.Failure($"Role '{role}' does not exist", "RoleNotFound");
                }
            }

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Add new roles
            var rolesToAdd = roles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to assign roles: {errors}", "RoleAssignmentFailed");
                }
            }

            // Update audit info
            var updatedAudit = user.Audit.Update(assignedBy, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Roles assigned successfully to user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
            return Result.Failure("An error occurred while assigning roles", "RoleAssignmentError");
        }
    }

    public async Task<Result> RemoveRolesAsync(string userId, List<string> roles, string removedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove specified roles
            var rolesToRemove = roles.Intersect(currentRoles).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return Result.Failure($"Failed to remove roles: {errors}", "RoleRemovalFailed");
                }
            }

            // Update audit info
            var updatedAudit = user.Audit.Update(removedBy, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Roles removed successfully from user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing roles from user: {UserId}, Roles: {Roles}", userId, string.Join(", ", roles));
            return Result.Failure("An error occurred while removing roles", "RoleRemovalError");
        }
    }

    public async Task<Result<List<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<List<string>>.Failure("User not found", "UserNotFound");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Result<List<string>>.Success(roles.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
            return Result<List<string>>.Failure("An error occurred while retrieving user roles", "RoleRetrievalError");
        }
    }

    public async Task<Result<bool>> UserHasRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not found", "UserNotFound");
            }

            var hasRole = await _userManager.IsInRoleAsync(user, role);
            return Result<bool>.Success(hasRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role for user: {UserId}, Role: {Role}", userId, role);
            return Result<bool>.Failure("An error occurred while checking user role", "RoleCheckError");
        }
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            if (user.EmailConfirmed)
            {
                return Result.Failure("Email is already confirmed", "EmailAlreadyConfirmed");
            }

            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            if (!confirmResult.Succeeded)
            {
                var errors = string.Join(", ", confirmResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to confirm email: {errors}", "EmailConfirmationFailed");
            }

            _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
            return Result.Failure("An error occurred while confirming the email", "EmailConfirmationError");
        }
    }

    public async Task<Result> SendEmailConfirmationAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found", "UserNotFound");
            }

            if (user.EmailConfirmed)
            {
                return Result.Failure("Email is already confirmed", "EmailAlreadyConfirmed");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = UrlEncoder.Default.Encode(token);

            // TODO: Implement email sending service
            // For now, just log that token was generated (without the actual token)
            _logger.LogInformation("Email confirmation token generated for user: {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation for user: {UserId}", userId);
            return Result.Failure("An error occurred while sending email confirmation", "EmailConfirmationError");
        }
    }

    public async Task<Result> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal if user exists or not
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return Result.Success();
            }

            if (!user.EmailConfirmed)
            {
                return Result.Failure("Email is not confirmed", "EmailNotConfirmed");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = UrlEncoder.Default.Encode(token);

            // TODO: Implement email sending service
            // For now, just log that token was generated (without the actual token)
            _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email for: {Email}", email);
            return Result.Failure("An error occurred while sending password reset email", "PasswordResetEmailError");
        }
    }

    public async Task<Result> ResetPasswordWithTokenAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result.Failure("Invalid email or token", "InvalidEmailOrToken");
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to reset password: {errors}", "PasswordResetFailed");
            }

            // Update password change timestamp
            await _accountManagementService.UpdatePasswordChangeAsync(user);

            _logger.LogInformation("Password reset with token successfully for user: {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token for email: {Email}", email);
            return Result.Failure("An error occurred while resetting the password", "PasswordResetError");
        }
    }

    public async Task<Result<AuthResultDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate refresh token
            if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            {
                return Result<AuthResultDto>.Failure("Invalid refresh token", "InvalidRefreshToken");
            }

            // TODO: Implement proper refresh token validation
            // For now, we'll return an error indicating refresh token functionality is not fully implemented
            return Result<AuthResultDto>.Failure("Refresh token functionality is not fully implemented", "RefreshTokenNotImplemented");

            // Note: The code below is commented out until proper refresh token validation is implemented
            /*
            // Find user by refresh token (you might need to implement this based on your token storage strategy)
            var user = await _userManager.FindByIdAsync(refreshTokenDto.RefreshToken);
            if (user == null)
            {
                return Result<AuthResultDto>.Failure("Invalid refresh token", "InvalidRefreshToken");
            }

            // Check if user can login
            if (!user.CanLogin())
            {
                if (user.IsAccountLocked)
                {
                    return Result<AuthResultDto>.Failure("Account is locked", "AccountLocked");
                }
                if (!user.IsActive)
                {
                    return Result<AuthResultDto>.Failure("Account is deactivated", "AccountDeactivated");
                }
                return Result<AuthResultDto>.Failure("Account is not accessible", "AccountInaccessible");
            }

            // Generate new access token
            var claims = await _accountManagementService.GetUserClaimsAsync(user);
            var accessToken = _accountManagementService.GenerateAccessToken(claims);
            var newRefreshToken = _accountManagementService.GenerateRefreshToken();

            // Update last login
            await _accountManagementService.UpdateLastLoginAsync(user);

            // Get user roles for mapping
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _userMapper.MapToUserDto(user, roles.ToList());

            // Create auth result
            var authResult = new AuthResultDto
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Configure from settings
                User = userDto
            };

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);
            return Result<AuthResultDto>.Success(authResult);
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Result<AuthResultDto>.Failure("An error occurred while refreshing the token", "RefreshTokenError");
        }
    }

    // Helper method to get user with roles
    private async Task<ApplicationUser> GetUserWithRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        // Populate roles for mapping
        var roles = await _userManager.GetRolesAsync(user);
        user.Roles = roles.ToList();
        
        return user;
    }

    // Helper method for password validation and handling
    private async Task<Result> ValidateAndSetPasswordAsync(ApplicationUser user, string password, string? currentPassword = null)
    {
        try
        {
            // Validate current password if provided
            if (!string.IsNullOrEmpty(currentPassword))
            {
                var isValidPassword = await _userManager.CheckPasswordAsync(user, currentPassword);
                if (!isValidPassword)
                {
                    return Result.Failure("Current password is incorrect", "InvalidCurrentPassword");
                }
            }

            // Validate new password using Identity's password validators
            var passwordValidators = _userManager.PasswordValidators;
            foreach (var validator in passwordValidators)
            {
                var validationResult = await validator.ValidateAsync(_userManager, user, password);
                if (!validationResult.Succeeded)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.Description));
                    return Result.Failure($"Password validation failed: {errors}", "PasswordValidationFailed");
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user: {UserId}", user.Id);
            return Result.Failure("An error occurred while validating the password", "PasswordValidationError");
        }
    }

    // Helper method for creating user with password
    private async Task<Result<ApplicationUser>> CreateUserWithPasswordAsync(ApplicationUser user, string password)
    {
        try
        {
            // Validate password
            var validationResult = await ValidateAndSetPasswordAsync(user, password);
            if (!validationResult.IsSuccess)
            {
                return Result<ApplicationUser>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);
            }

            // Create user with password
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<ApplicationUser>.Failure($"Failed to create user: {errors}", "UserCreationFailed");
            }

            return Result<ApplicationUser>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with password: {Email}", user.Email);
            return Result<ApplicationUser>.Failure("An error occurred while creating the user", "UserCreationError");
        }
    }

    // Helper method for changing password
    private async Task<Result> ChangeUserPasswordAsync(ApplicationUser user, string newPassword, string? currentPassword = null)
    {
        try
        {
            // Validate password
            var validationResult = await ValidateAndSetPasswordAsync(user, newPassword, currentPassword);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // Change password
            var changeResult = await _userManager.ChangePasswordAsync(user, currentPassword ?? "", newPassword);
            if (!changeResult.Succeeded)
            {
                var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to change password: {errors}", "PasswordChangeFailed");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", user.Id);
            return Result.Failure("An error occurred while changing the password", "PasswordChangeError");
        }
    }
} 