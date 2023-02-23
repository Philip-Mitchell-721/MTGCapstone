using Microsoft.AspNetCore.Http;
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

        public AuthenticationController(CapstoneDbContext capstoneDbContext,
            IConfiguration configuration)
        {
            _capstoneDbContext = capstoneDbContext 
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _configuration = configuration 
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(
            AuthenticationRequestBody authenticationRequestBody)
        {
            //Validate the Username/password
            var user = ValidateUserCredentials(
                authenticationRequestBody.UserName,
                authenticationRequestBody.Password);
            //TODO: Make sure that password in database is Hashed/Encoding.  Look at Identity manager.

            if (user is null)
                return Unauthorized();

            //Create Key and Credentials
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"])); 
            var signingCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

            //Make the claims
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("user_name", user.UserName));

            //Create the token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

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

        private User? ValidateUserCredentials(string? userName, string? password)
        {
            //var hashedPassword = //hash passed in password
            //Move this into it's own service.
            var user = _capstoneDbContext.Users.FirstOrDefault(user => 
                user.UserName == userName && user.Password == hashedPassword);
            
            return user;
        }
    }
}
