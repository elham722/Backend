using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Services
{
    public interface IAuthApiClient
    {
        
        Task<AuthResultDto> RegisterAsync(RegisterDto dto);

        Task<AuthResultDto> LoginAsync(LoginDto dto);

        Task<bool> LogoutAsync(LogoutDto? logoutDto = null);

        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);

        Task<bool> ValidateTokenAsync();
    }
} 