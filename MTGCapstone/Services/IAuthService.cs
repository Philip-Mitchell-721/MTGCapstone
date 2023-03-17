using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;

namespace MTGCapstone.API.Services
{
    public interface IAuthService
    {

        Task<TokenResponse> LoginAsync(string userName, string password);
        Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        

    }
}