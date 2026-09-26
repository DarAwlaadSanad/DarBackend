using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IAuthService
    {
        Task<(bool Succeeded, IEnumerable<string>? Errors)> RegisterAsync(RegisterDTO model);
        Task<LoginResultDTO?> LoginAsync(LoginDTO model);
        Task<LoginResultDTO?> RefreshTokenAsync(RefreshTokenRequestDTO model);
    }
}
