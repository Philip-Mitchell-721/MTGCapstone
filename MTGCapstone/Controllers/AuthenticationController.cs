using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Responses;
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
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService
                ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response<TokenDTO>>> Authenticate(
            [FromBody] AuthenticationRequestBody authenticationRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<TokenDTO> tokenResponse = await _authService.LoginAsync(authenticationRequestBody);

            if (!tokenResponse.Success)
            {
                return Unauthorized(tokenResponse.Errors);
            }

            return Ok(tokenResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationModel userRegistrationModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //TODO: Finish updating to Response<TokenResponse>, then update interface.
                Response<TokenDTO> tokenResponse = await _authService.RegisterUserAsync(userRegistrationModel);

                if (!tokenResponse.Success)
                {
                    return StatusCode((int)ResponseStatusCodes.BadRequest, tokenResponse.Errors);
                }

                //TODO: Remember that I need to change this to return the email link to confirm email account.
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user");
                return StatusCode(500, "Error while registering user");
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<Response<TokenDTO>>> Refresh(RefreshTokenDTO refreshTokenDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<TokenDTO> tokenResponse = await _authService.RefreshTokensAsync(refreshTokenDTO);

            if (!tokenResponse.Success)
            {
                return Unauthorized(tokenResponse.Errors);
            }

            return Ok(tokenResponse);
        }

        [HttpPost("revoke")]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> Revoke(RefreshTokenToRevokeDTO refreshToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Response<TokenDTO> response = await _authService.RevokeAsync(refreshToken);

            if (!response.Success)
            {
                return BadRequest(response.Errors);
            }

            return NoContent();
        }

        [HttpPost("confirm-email-request")]
        public async Task<IActionResult> ConfirmEmailRequestAsync(ConfirmEmailRequestDTO userName)
        {

            System.Security.Claims.Claim? id = User.Claims.FirstOrDefault(c => c.Type == "sub");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<TokenDTO> response = await _authService.ConfirmEmailRequestAsync(userName);

            if (!response.Success)
            {
                return BadRequest(response.Errors);
            }

            return Ok();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] ConfirmEmailDTO confirmEmailDTO)
        {
            //ASK: Should this be from Query, since it's a link in an email?
            //but the one for change password should be from body like normal, because the link will 
            //redirect them to the page with the form to change password?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<TokenDTO> response = await _authService.ConfirmEmailAsync(confirmEmailDTO);

            if (!response.Success)
            {
                return BadRequest(response.Errors);
            }

            return Ok();
        }

        [HttpPost("change-password-request")]
        public async Task<IActionResult> ChangePasswordRequest(ChangePasswordRequestDTO userName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<TokenDTO> response = await _authService.ChangePasswordRequestAsync(userName);

            if (!response.Success || response.Value is null)
            {
                return BadRequest(response.Errors);
            }
            //This is just to see the changePasswordEmailToken
            //remove this and return Ok()
            return Ok(response.Value.AccessToken);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<TokenDTO> response = await _authService.ChangePasswordAsync(changePasswordDTO);

            if (!response.Success)
            {
                return BadRequest(response.Errors);
            }

            return Ok(); 
        }

    }
}
