using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using MediatR;

namespace Backend.Application.Features.UserManagement.Commands.Logout
{
    /// <summary>
    /// Command to logout user and invalidate refresh tokens
    /// </summary>
    public class LogoutCommand : IRequest<Result<LogoutResultDto>>
    {
        /// <summary>
        /// Refresh token to invalidate
        /// </summary>
        public string? RefreshToken { get; set; }
        
        /// <summary>
        /// Whether to logout from all devices
        /// </summary>
        public bool LogoutFromAllDevices { get; set; } = false;
        
        /// <summary>
        /// User ID (optional, can be extracted from refresh token)
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// IP address of the request
        /// </summary>
        public string? IpAddress { get; set; }
        
        /// <summary>
        /// User agent of the request
        /// </summary>
        public string? UserAgent { get; set; }
        
        /// <summary>
        /// Reason for logout (optional)
        /// </summary>
        public string? LogoutReason { get; set; }
    }
} 