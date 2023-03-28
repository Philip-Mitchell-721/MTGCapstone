using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;

namespace MTGCapstone.API.Services
{
    public interface IAuthService
    {

        Task<TokenResponse> RegisterUserAsync(UserRegistrationModel userRegistrationModel);
        Task<TokenResponse> LoginAsync(AuthenticationRequestBody authenticationRequestBody);
        Task<TokenResponse> RefreshTokensAsync(RefreshTokenDTO refreshTokenDTO);
        Task<TokenResponse> RevokeAsync(RefreshTokenToRevokeDTO refreshToken);
        Task<TokenResponse> ConfirmEmailRequestAsync(ConfirmEmailRequestDTO confirmEmailRequestDTO);
        Task<TokenResponse> ConfirmEmailAsync(ConfirmEmailDTO confirmEmailDTO);
        Task<TokenResponse> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName);
        Task<TokenResponse> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO);
    }
}