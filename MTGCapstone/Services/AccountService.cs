using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.DbContexts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MTGCapstone.API.Services
{
    public class AccountService: IAccountService
    {
        private readonly IConfiguration _configuration;
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly UserManager<User> _userManager;

        public AccountService(IConfiguration configuration,
            CapstoneDbContext capstoneDbContext,
            UserManager<User> userManager)
        {
            _configuration = configuration 
                ?? throw new ArgumentNullException(nameof(configuration));
            _capstoneDbContext = capstoneDbContext 
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _userManager = userManager 
                ?? throw new ArgumentNullException(nameof(userManager));
        }
        public string CreateJwt(User user)
        {
            //Create Key and Credentials
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            var signingCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

            //Make the claims
            //if (user.Id is 0 || user.UserName is null)
            //{
            //    return StatusCode(500);
            //}

            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("user_name", user.UserName));

            //Create the token
            var jwtSecurityToken = new JwtSecurityToken
                (
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    claims: claimsForToken,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: signingCredentials
                );

            //Write the token
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return tokenToReturn;
        }

        public async Task<User?> ValidateUserCredentialsAsync(string? userName, string? password)
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
