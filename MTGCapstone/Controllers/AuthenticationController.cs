using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
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
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthenticationController(CapstoneDbContext capstoneDbContext,
            IConfiguration configuration,
            UserManager<User> userManager,
            IPasswordHasher<User> passwordHasher)
        {
            _capstoneDbContext = capstoneDbContext 
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _configuration = configuration 
                ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager 
                ?? throw new ArgumentNullException(nameof(userManager));
            _passwordHasher = passwordHasher 
                ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<string>> Authenticate(
            AuthenticationRequestBody authenticationRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return Unauthorized();
            }
            //Validate the Username/password
            var user = await ValidateUserCredentials(
                authenticationRequestBody.UserName,
                authenticationRequestBody.Password);

            if (user is null)
                return Unauthorized();

            //Create Key and Credentials
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"])); 
            var signingCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

            //Make the claims
            if (user.Id is 0 || user.UserName is null)
            {
                return StatusCode(500);
            }

            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("user_name", user.UserName));
            
            //Create the token
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims: claimsForToken,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials);

            //Write the token
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);


        }

        //This class won't be used outside of this controller, so we can scope it here
        //or move it into it's own folder.
        public class AuthenticationRequestBody
        {
            [Required]
            public string? UserName { get; set; }

            [Required]
            public string? Password { get; set; }
        }

        private async Task<User?> ValidateUserCredentials(string? userName, string? password)
        {
            //_userManager.CheckPasswordAsync
            var user = _capstoneDbContext.Users.FirstOrDefault(user =>
               user.UserName == userName);

            //Could also check the password this way?
            //_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (user == null || await _userManager.CheckPasswordAsync(user, password))
                return null;

            return user;
        }
    }
}
