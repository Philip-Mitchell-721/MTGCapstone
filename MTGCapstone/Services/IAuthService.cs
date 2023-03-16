using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Tokens;

namespace MTGCapstone.API.Services
{
    public interface IAuthService
    {
        string CreateAccessToken(User user);
        Task<User?> ValidateUserCredentialsAsync(string? userName, string? password);
        Task<TokenResponse> RefreshToken(RefreshTokenDTO refresh);

    }
}