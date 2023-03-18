using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;
using MTGCapstone.API.DbContexts;
using NuGet.Packaging.Signing;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
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
        private readonly IMapper _mapper;

        public AuthService(IConfiguration configuration,
            CapstoneDbContext capstoneDbContext,
            UserManager<User> userManager,
            PasswordHasher<User> passwordHasher,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            RoleManager<IdentityRole<int>> roleManager,
            IMapper mapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _capstoneDbContext = capstoneDbContext ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler ?? throw new ArgumentNullException(nameof(jwtSecurityTokenHandler));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<TokenResponse> RegisterUserAsync(UserRegistrationModel userRegistrationModel)
        {
           
            //Check for existing user with username
            var existingUser = await _userManager.FindByNameAsync(userRegistrationModel.UserName);
            if (existingUser is not null)
                return new TokenResponse { Error = "User with this Username already exists" };

            //Create new user
            User user = _mapper.Map<User>(userRegistrationModel);
            IdentityResult result = await _userManager.CreateAsync(user, userRegistrationModel.Password);
            if (!result.Succeeded)
            {
                var sb = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    sb.AppendLine(error.ToString());
                }
                
                return new TokenResponse { Error = sb.ToString() };
            }

            //return tokens for the new user
            return await CreateNewTokensAsync(user);
        }
        public async Task<TokenResponse> LoginAsync(string userName, string password)
        {
            var user = await ValidateUserCredentialsAsync(userName, password);

            if (user is null)
                return new TokenResponse { Error = "Invalid user credentials." };

            return await CreateNewTokensAsync(user);
        }

        public async Task<TokenResponse> RefreshTokensAsync(string accessToken, string refreshToken)
        {
            //Validate Access Token Expiration
            JwtSecurityToken token = _jwtSecurityTokenHandler.ReadJwtToken(accessToken);
            if (token.ValidTo >= DateTime.UtcNow)
                return new TokenResponse { Error = "Access token hasn't expired yet" };

            //Validate Refresh Token
            var oldRefreshToken = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refreshToken);
            if (oldRefreshToken is null)
                return new TokenResponse { Error = "Invalid refresh token" };
            if (DateTime.UtcNow > oldRefreshToken.ExpiredAt)
                return new TokenResponse { Error = "Expired refresh token" };
            if (oldRefreshToken.JwtId != token.Id)
                return new TokenResponse { Error = "Tokens don't match" };
            if (oldRefreshToken.Used)
                return new TokenResponse { Error = "Previously used refresh token" };
            if (oldRefreshToken.Revoked)
                return new TokenResponse { Error = "Revoked refresh token" };
            
            //Update old Refresh Token
            oldRefreshToken.Used = true;
            await _capstoneDbContext.SaveChangesAsync();

            var user = await _capstoneDbContext.Users.FindAsync(oldRefreshToken.UserId);
            if (user is null)
                return new TokenResponse { Error = "User not found" };

            return await CreateNewTokensAsync(user);
        }
        public async Task<TokenResponse> RevokeAsync(string refreshToken)
        {
            var tokenToRevoke = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refreshToken);
            if (tokenToRevoke is null)
                return new TokenResponse { Error = "Invalid refresh token" };
            //Could check to make sure that it's an otherwise valid refresh token, but this still works.

            tokenToRevoke.Revoked = true;
            await _capstoneDbContext.SaveChangesAsync();

            return new TokenResponse { Success = true };
        }

        public async Task<TokenResponse> ChangePasswordAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new TokenResponse { Error = "" };
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            string baseUrl = "https://localhost:7277"; //TODO: Make this changable with config.
            string resetTokenUrl = $"{baseUrl}?email={user.Email}&token={resetToken}";
            string emailBody = $"<a href=\"{resetTokenUrl}\" >Reset Password</a>";

            var email = new MailMessage()
                {
                    Body = emailBody,
                    Subject = _identitySetting.CurrentValue.PasswordResetSubject,
                    Recipients = new List<string>() { user.Email }
                };
            await _mailer.SendEmailAsync(emailMessage);
        }

        private async Task<User?> ValidateUserCredentialsAsync(string userName, string password)
        {
            var user = _capstoneDbContext.Users.FirstOrDefault(user =>
               user.UserName == userName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                return null;

            return user;
        }

        private async Task<TokenResponse> CreateNewTokensAsync(User user)
        {
            SigningCredentials signingCredentials = CreateSigningCredentials();

            List<Claim> claimsForToken = await CreateClaimsAsync(user);

            //Create the accessToken
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken
                (
                    issuer: _configuration["Authentication:Issuer"],
                    audience: _configuration["Authentication:Audience"],
                    claims: claimsForToken,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: signingCredentials

                );


            //Write the accessToken
            string accessToken = _jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);


            string refreshToken = await CreateRefreshTokenAsync(user, jwtSecurityToken.Id);

            return new TokenResponse { Success = true, AccessToken = accessToken, RefreshToken = refreshToken };
        }

        private async Task<List<Claim>> CreateClaimsAsync(User user)
        {
            //Make the claims
            var claimsForToken = new List<Claim>
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("jti", Guid.NewGuid().ToString())
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

            return claimsForToken;
        }

        private SigningCredentials CreateSigningCredentials()
        {
            //Create Key and Credentials
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            //ASK: Where should I store these secrets?  How to use environment variables.

            var signingCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);
            return signingCredentials;
        }

        private async Task<string> CreateRefreshTokenAsync(User user, string jwtId)
        {
            
            var refreshToken = new RefreshToken()
            {
                Token = _passwordHasher.HashPassword(user, Guid.NewGuid().ToString()),
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                JwtId = jwtId
            };

            _capstoneDbContext.RefreshTokens.Add(refreshToken);
            await _capstoneDbContext.SaveChangesAsync();

            return refreshToken.Token;
        }



    }
}
