using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Identity.Models;
using Backend.Identity.Context;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Identity.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BackendIdentityDbContext _context;
        private readonly IUserService _userService;

        public JwtTokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            BackendIdentityDbContext context,
            IUserService userService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<string> GenerateAccessTokenAsync(IApplicationUser user, CancellationToken cancellationToken = default)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(ClaimTypes.Email, user.Email ?? ""),
                new("sub", user.Id),
                new("jti", Guid.NewGuid().ToString()),
                new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add user roles
            var userRoles = await _userService.GetUserRolesAsync(user.Id, cancellationToken);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add user permissions
            var userPermissions = await _userService.GetUserPermissionsAsync(user.Id, cancellationToken);
            foreach (var permission in userPermissions)
            {
                claims.Add(new Claim("permission", $"{permission.Resource}:{permission.Action}"));
            }

            // Add user claims - we'll get these from the user's claims collection
            // Note: For now, we'll skip direct claims as they're not easily accessible through IApplicationUser
            // This could be enhanced by adding a GetUserClaimsAsync method to IUserService if needed

            // Add branch ID if available
            var branchId = await _userService.GetUserBranchIdAsync(user.Id, cancellationToken);
            if (!string.IsNullOrEmpty(branchId))
            {
                claims.Add(new Claim("BranchId", branchId));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var refreshToken = Guid.NewGuid().ToString();
            return refreshToken;
        }

        public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"))),
                ValidateLifetime = false, // We don't care about the expired token
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(ut => ut.UserId == userId && 
                                         ut.Name == "RefreshToken" && 
                                         ut.Value == refreshToken, 
                                    cancellationToken);

            return userToken != null;
        }

        public async Task RevokeRefreshTokenAsync(string userId, string refreshToken, string revokedBy, string? reason = null, CancellationToken cancellationToken = default)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(ut => ut.UserId == userId && 
                                         ut.Name == "RefreshToken" && 
                                         ut.Value == refreshToken, 
                                    cancellationToken);

            if (userToken != null)
            {
                // For default Identity classes, we can't revoke, so we'll delete the token
                _context.UserTokens.Remove(userToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RevokeAllRefreshTokensAsync(string userId, string revokedBy, string? reason = null, CancellationToken cancellationToken = default)
        {
            var userTokens = await _context.UserTokens
                .Where(ut => ut.UserId == userId && 
                           ut.Name == "RefreshToken")
                .ToListAsync(cancellationToken);

            foreach (var token in userTokens)
            {
                _context.UserTokens.Remove(token);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}