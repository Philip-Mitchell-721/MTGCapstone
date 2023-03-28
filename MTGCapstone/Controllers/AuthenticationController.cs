using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;
using IAuthService = MTGCapstone.API.Services.IAuthService;

namespace MTGCapstone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        

        public AuthenticationController(IAuthService authService)
        {
            _authService = authService 
                ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Authenticate(
            [FromBody] AuthenticationRequestBody authenticationRequestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TokenResponse tokenResponse = await _authService.LoginAsync(authenticationRequestBody);

            if (!tokenResponse.Success)
                return Unauthorized(tokenResponse.Error);
            

            return Ok(tokenResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationModel userRegistrationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokenResponse = await _authService.RegisterUserAsync(userRegistrationModel);
            if (!tokenResponse.Success)
                return StatusCode(500);
            //TODO: Consider if registering should log user in or prompt them to confirm email.
            return Ok(tokenResponse);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponse>> Refresh(RefreshTokenDTO refreshTokenDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TokenResponse tokenResponse = await _authService.RefreshTokensAsync(refreshTokenDTO);

            if (!tokenResponse.Success)
                return Unauthorized(tokenResponse.Error);
            
            return Ok(tokenResponse);
        }

        [HttpPost("revoke")]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Revoke(RefreshTokenToRevokeDTO refreshToken)
        {
            if (!ModelState.IsValid)
            {
                //TODO: fix this to check modelstate once I have a DTO setup
                return BadRequest();
            }

            var response = await _authService.RevokeAsync(refreshToken);

            if (!response.Success)
                return BadRequest(response.Error);

            return NoContent();
        }

        [HttpPost("confirm-email-request")]
        public async Task<IActionResult> ConfirmEmailRequestAsync(ConfirmEmailRequestDTO userName)
        {

            var id = User.Claims.FirstOrDefault(c => c.Type == "sub");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.ConfirmEmailRequestAsync(userName);

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync(ConfirmEmailDTO confirmEmailDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.ConfirmEmailAsync(confirmEmailDTO);

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok();
        }

        [HttpPost("change-password-request")]
        public async Task<IActionResult> ChangePasswordRequest(ChangePasswordRequestDTO userName)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.ChangePasswordRequestAsync(userName);

            if (!response.Success)
                return BadRequest(response.Error);
            //This is just to see the changePasswordEmailToken
            //remove this and return Ok()
            return Ok(response.AccessToken);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.ChangePasswordAsync(changePasswordDTO);

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok(); 
        }

    }
}
