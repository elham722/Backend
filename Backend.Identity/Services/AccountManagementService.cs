using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Services
{
    public interface IAccountManagementService
    {
        Task<bool> UpdateLastLoginAsync(ApplicationUser user);
        Task<bool> UpdatePasswordChangeAsync(ApplicationUser user);
        Task<bool> IncrementLoginAttemptsAsync(ApplicationUser user);
        Task<bool> DeactivateAccountAsync(ApplicationUser user);
        Task<bool> ActivateAccountAsync(ApplicationUser user);
        Task<bool> MarkAsDeletedAsync(ApplicationUser user);
        Task<bool> LinkToCustomerAsync(ApplicationUser user, Guid customerId);
        Task<bool> UnlinkFromCustomerAsync(ApplicationUser user);
        Task<List<Claim>> GetUserClaimsAsync(ApplicationUser user);
        string GenerateAccessToken(List<Claim> claims);
        string GenerateRefreshToken();
    }

    public class AccountManagementService : IAccountManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTimeService _dateTimeService;
        private readonly IConfiguration _configuration;

        public AccountManagementService(
            UserManager<ApplicationUser> userManager, 
            IDateTimeService dateTimeService,
            IConfiguration configuration)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> UpdateLastLoginAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.UpdateLastLogin(_dateTimeService);
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdatePasswordChangeAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.UpdatePasswordChange(_dateTimeService);
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IncrementLoginAttemptsAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.IncrementLoginAttempts();
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeactivateAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.Deactivate();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ActivateAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.Activate();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> MarkAsDeletedAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.MarkAsDeleted(_dateTimeService);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> LinkToCustomerAsync(ApplicationUser user, Guid customerId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetCustomerId(customerId);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UnlinkFromCustomerAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetCustomerId(null);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<List<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("UserId", user.Id),
                new Claim("UserName", user.UserName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("EmailConfirmed", user.EmailConfirmed.ToString()),
                new Claim("IsActive", user.IsActive.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
                new Claim("PhoneNumberConfirmed", user.PhoneNumberConfirmed.ToString())
            };

            // Add roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        public string GenerateAccessToken(List<Claim> claims)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = _configuration["JwtSettings:Issuer"] ?? "Backend.Api";
            var audience = _configuration["JwtSettings:Audience"] ?? "Backend.Client";
            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60);

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + "_" + DateTime.UtcNow.Ticks;
        }
    }
} 