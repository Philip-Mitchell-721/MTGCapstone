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
        Task<TokenResponse> ConfirmEmailRequestAsync(User user);
        Task<TokenResponse> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName);
        Task<TokenResponse> ConfirmEmailAsync(ConfirmEmailDTO confirmEmailDTO);
        Task<TokenResponse> NewPasswordAsync(ResetPasswordDTO resetPasswordDTO);
    }
}