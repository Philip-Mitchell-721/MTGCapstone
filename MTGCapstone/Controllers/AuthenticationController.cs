using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;
using MTGCapstone.API.DbContexts;
using IAuthService = MTGCapstone.API.Services.IAuthService;

namespace MTGCapstone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public AuthenticationController(IAuthService authService,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _authService = authService 
                ?? throw new ArgumentNullException(nameof(authService));
            _userManager = userManager 
                ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper 
                ?? throw new ArgumentNullException(nameof(mapper));
            
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
                return StatusCode(500, tokenResponse.Error);
            //ASK: probably should not return the errors here, but unsure what TO return

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

        [HttpPost("change-password-request")]
        public async Task<IActionResult> ChangePasswordRequest(ChangePasswordRequestDTO userName) //TODO: make DTO with errormessage in annotation
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.ChangePasswordRequestAsync(userName);

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok(response.AccessToken);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(string changeEmailToken, string email, string newPassword, string confirmedNewPassword)
        {
            return Ok(); 
        }

    }
}
