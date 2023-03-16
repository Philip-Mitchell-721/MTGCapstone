using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Tokens;
using MTGCapstone.API.DbContexts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MTGCapstone.API.Services
{
    public class AuthService: IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly UserManager<User> _userManager;
        private readonly TokenHandler _tokenHandler;

        public AuthService(IConfiguration configuration,
            CapstoneDbContext capstoneDbContext,
            UserManager<User> userManager,
            TokenHandler tokenHandler)
        {
            _configuration = configuration 
                ?? throw new ArgumentNullException(nameof(configuration));
            _capstoneDbContext = capstoneDbContext 
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _userManager = userManager 
                ?? throw new ArgumentNullException(nameof(userManager));
            _tokenHandler = tokenHandler 
                ?? throw new ArgumentNullException(nameof(tokenHandler));
        }
        public string CreateAccessToken(User user)
        {
            //Create Key and Credentials
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            //ASK: Where should I store these secrets?  How to use environment variables.

            var signingCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);

            //Make the claims
            var claimsForToken = new List<Claim>
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("user_name", user.UserName)
            };

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

        public async Task<TokenResponse> RefreshAccessToken(RefreshTokenDTO refresh)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            //TokenValidationParameters tokenValidationParameters = new TokenValidationParameters();
            //ClaimsPrincipal principal = handler.ValidateToken(refresh.Token, tokenValidationParameters, out SecurityToken? validatedToken);
            JwtSecurityToken token = handler.ReadJwtToken(refresh.Token);
            
            if (token.ValidTo >= DateTime.UtcNow)
            {
                return new TokenResponse(false, "Access Token hasn't expired yet", token);
            }
            var oldRefreshToken = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refresh.Token);
        }

        public Task<TokenResponse> RefreshToken(RefreshTokenDTO refresh)
        {

        }
    }
}
