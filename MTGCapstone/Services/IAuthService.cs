using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Responses;
using MTGCapstone.API.Data.Tokens;

namespace MTGCapstone.API.Services
{
    public interface IAuthService
    {

        Task<Response<TokenDTO>> RegisterUserAsync(UserRegistrationModel userRegistrationModel);
        Task<Response<TokenDTO>> LoginAsync(AuthenticationRequestBody authenticationRequestBody);
        Task<Response<TokenDTO>> RefreshTokensAsync(RefreshTokenDTO refreshTokenDTO);
        Task<Response<TokenDTO>> RevokeAsync(RefreshTokenToRevokeDTO refreshToken);
        Task<Response<TokenDTO>> ConfirmEmailRequestAsync(ConfirmEmailRequestDTO confirmEmailRequestDTO);
        Task<Response<TokenDTO>> ConfirmEmailAsync(ConfirmEmailDTO confirmEmailDTO);
        Task<Response<TokenDTO>> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName);
        Task<Response<TokenDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO);
    }
}