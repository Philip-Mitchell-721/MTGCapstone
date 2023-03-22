using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;

namespace MTGCapstone.API.Services
{
    public interface IAuthService
    {

        Task<TokenResponse> LoginAsync(AuthenticationRequestBody authenticationRequestBody);
        Task<TokenResponse> RefreshTokensAsync(RefreshTokenDTO refreshTokenDTO);
        Task<TokenResponse> RegisterUserAsync(UserRegistrationModel userRegistrationModel);
        Task<TokenResponse> RevokeAsync(RefreshTokenToRevokeDTO refreshToken);
        Task<TokenResponse> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName);
    }
}