using Microsoft.AspNetCore.Identity;
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
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly UserManager<User> _userManager;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AuthService(IConfiguration configuration,
            CapstoneDbContext capstoneDbContext,
            UserManager<User> userManager,
            PasswordHasher<User> passwordHasher,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
            _capstoneDbContext = capstoneDbContext
                ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _userManager = userManager
                ?? throw new ArgumentNullException(nameof(userManager));
            _passwordHasher = passwordHasher
                ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler 
                ?? throw new ArgumentNullException(nameof(jwtSecurityTokenHandler));
            _roleManager = roleManager 
                ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<TokenResponse> LoginAsync(string userName, string password)
        {
            var user = await ValidateUserCredentialsAsync(userName, password);

            if (user is null)
                return new TokenResponse { Error = "Invalid user credentials." };

            var accessToken = await CreateAccessToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user);

            return new TokenResponse { AccessToken = accessToken, RefreshToken = refreshToken, Success = true };
        }


        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenDTO refresh)
        {
            
            JwtSecurityToken token = _jwtSecurityTokenHandler.ReadJwtToken(refresh.Token);

            if (token.ValidTo >= DateTime.UtcNow)
                return new TokenResponse { Error = "Access Token hasn't expired yet" };

            var oldRefreshToken = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refresh.Token);
        }

        private async Task<User?> ValidateUserCredentialsAsync(string userName, string password)
        {
            var user = _capstoneDbContext.Users.FirstOrDefault(user =>
               user.UserName == userName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                return null;

            return user;
        }

        private async Task<string> CreateAccessToken(User user)
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
            var userClaims = await _userManager.GetClaimsAsync(user);
            claimsForToken.AddRange(userClaims);
            
            //Add roles to the claims
            //ASK: Are the role claims and permissions not already in the list of user claims?
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claimsForToken.Add(new Claim(ClaimTypes.Role, userRole));
                IdentityRole<int> role = await _roleManager.FindByNameAsync(userRole);
                if (role == null)
                {
                    continue;
                }
                IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);

                foreach (Claim roleClaim in roleClaims)
                {
                    if (claimsForToken.Contains(roleClaim))
                    {
                        continue;
                    }
                    claimsForToken.Add(roleClaim);
                }
            }

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
        private async Task<string> CreateRefreshTokenAsync(User user)
        {
            
            var refreshToken = new RefreshToken()
            {
                Token = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString()),
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
            };

            _capstoneDbContext.RefreshTokens.Add(refreshToken);
            await _capstoneDbContext.SaveChangesAsync();

            return refreshToken.Token;
        }
    }
}
