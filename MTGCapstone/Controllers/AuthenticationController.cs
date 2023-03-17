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
        public async Task<ActionResult<TokenResponse>> Authenticate(
            AuthenticationRequestBody authenticationRequestBody)
        {
            if (!ModelState.IsValid || authenticationRequestBody.UserName is null || authenticationRequestBody.Password is null)
                return BadRequest(ModelState);

            TokenResponse tokenResponse = await _authService.LoginAsync(authenticationRequestBody.UserName, authenticationRequestBody.Password);

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

            

            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponse>> Refresh(RefreshTokenDTO refresh)
        {
            if (!ModelState.IsValid 
                || String.IsNullOrWhiteSpace(refresh.AccessToken) 
                || String.IsNullOrWhiteSpace(refresh.RefreshToken))
                return BadRequest(ModelState);

            TokenResponse tokenResponse = await _authService.RefreshTokensAsync(refresh.AccessToken, refresh.RefreshToken);

            if (!tokenResponse.Success)
                return Unauthorized(tokenResponse.Error);
            
            return Ok(tokenResponse);
        }
    }
}
