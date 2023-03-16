using AutoMapper;
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
        public async Task<ActionResult<string>> Authenticate(
            AuthenticationRequestBody authenticationRequestBody)
        {
            if (!ModelState.IsValid)
                return Unauthorized();

            TokenResponse tokenResponse = _authService.Login();
            ////Validate the Username/password
            //var user = await _authService.ValidateUserCredentialsAsync(
            //    authenticationRequestBody.UserName,
            //    authenticationRequestBody.Password);

            //if (user is null) 
            //    return Unauthorized();

            ////Create Access Token
            //var tokenToReturn = _authService.CreateAccessToken(user);

            if (!tokenResponse.Success)
            {
                return Unauthorized();
            }

            return Ok(tokenToReturn);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationModel userRegistrationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(userRegistrationModel);

            var result = await _userManager.CreateAsync(user, userRegistrationModel.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return StatusCode(500, ModelState); 
            }

            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponse>> Refresh(RefreshTokenDTO refresh)
        {
            if (!ModelState.IsValid || String.IsNullOrWhiteSpace(refresh.Token) || String.IsNullOrWhiteSpace(refresh.RefreshToken))
                return BadRequest(ModelState);

            // get tokenResponse from service
            var tokenResponse = new TokenResponse(false, "", new AccessToken("", 0, new RefreshToken("", 0)));
            //await _authService.RefreshToken(refresh);

            if (!tokenResponse.Success)
                return BadRequest(tokenResponse.Message);
            if (tokenResponse.Token is null)
                return StatusCode(500);
            //Don't want refresh token to be part of access token. TODO: fix this
            return Ok(tokenResponse.Token);
        }
    }
}
