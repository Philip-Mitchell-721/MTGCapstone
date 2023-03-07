using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Services;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MTGCapstone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        //TODO: Remove injected services that have now been moved.

        public AuthenticationController(IAccountService accountService,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _accountService = accountService 
                ?? throw new ArgumentNullException(nameof(accountService));
            _userManager = userManager 
                ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper 
                ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("signin")]
        public async Task<ActionResult<string>> Authenticate(
            AuthenticationRequestBody authenticationRequestBody)
        {
            if (!ModelState.IsValid)
                return Unauthorized();
            
            //Validate the Username/password
            var user = await _accountService.ValidateUserCredentialsAsync(
                authenticationRequestBody.UserName,
                authenticationRequestBody.Password);

            if (user is null) 
                return Unauthorized();

            //Create Access Token
            var tokenToReturn = _accountService.CreateJwt(user);

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

                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
