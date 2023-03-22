using AutoMapper;
using FluentEmail.Core;
using FluentEmail.Smtp;
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
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IMapper _mapper;

        public AuthService(IConfiguration configuration,
            CapstoneDbContext capstoneDbContext,
            UserManager<User> userManager,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            RoleManager<IdentityRole<int>> roleManager,
            IMapper mapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _capstoneDbContext = capstoneDbContext ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
        public async Task<TokenResponse> LoginAsync(AuthenticationRequestBody authenticationRequestBody)
        {
            var user = await ValidateUserCredentialsAsync(
                authenticationRequestBody.UserName, 
                authenticationRequestBody.Password);

            if (user is null)
                return new TokenResponse { Error = "Invalid user credentials." };

            return await CreateNewTokensAsync(user);
        }

        public async Task<TokenResponse> RefreshTokensAsync(RefreshTokenDTO refreshTokenDTO)
        {
            //Validate Access Token Expiration
            JwtSecurityToken token = _jwtSecurityTokenHandler.ReadJwtToken(refreshTokenDTO.AccessToken);
            if (token.ValidTo >= DateTime.UtcNow)
                return new TokenResponse { Error = "Access token hasn't expired yet" };

            //Validate Refresh Token
            var oldRefreshToken = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refreshTokenDTO.RefreshToken);
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
        public async Task<TokenResponse> RevokeAsync(RefreshTokenToRevokeDTO refreshToken)
        {
            var tokenToRevoke = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refreshToken.RefreshToken);
            if (tokenToRevoke is null)
                return new TokenResponse { Error = "Invalid refresh token" };
            //Could check to make sure that it's an otherwise valid refresh token, but this still works.

            tokenToRevoke.Revoked = true;
            await _capstoneDbContext.SaveChangesAsync();

            return new TokenResponse { Success = true };
        }

        public async Task<TokenResponse> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName)
        {
            var user = await _userManager.FindByNameAsync(userName.UserName);
            if (user == null)
                return new TokenResponse { Error = "User not found" };
            
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            //TODO: Make this changable with config.
            //TODO: Change this to whatever url my front end needs to change password.
            string baseUrl = "https://localhost:7277/api/authentication/change-password"; 
            string resetTokenUrl = $"{baseUrl}?email={user.Email}&token={resetToken}";
            string emailBody = $"<a href=\"{resetTokenUrl}\" >Reset Password</a>";

            //TODO: local host is for testing.  Change this to your email server.
            var sender = new SmtpSender(() => new SmtpClient("localhost") 
            {
                EnableSsl = false, //For testing
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = @"C:\Users\Philip\Documents\EmailTest"
            });

            Email.DefaultSender = sender;
            //TODO: still need to add fluentemail to DI container.

            var email = await Email
                .From("noreply@MTGCapstone.com")
                .To(user.Email, user.UserName)
                .Subject("CapstoneMTG Password Reset")
                .Body(emailBody, true)
                .SendAsync();

            if (!email.Successful)
            {
                var sb = new StringBuilder();
                foreach (var error in email.ErrorMessages)
                {
                    sb.AppendLine(error.ToString());
                }

                return new TokenResponse { Error = sb.ToString() };
            }
            
            //only returning this right now for testing.  probably only return success=true.
            return new TokenResponse { Success = true, AccessToken = emailBody };
        }

        private async Task<User?> ValidateUserCredentialsAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                //_logger.log()
                return null;
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
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
                    expires: DateTime.UtcNow.AddSeconds(7200),
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
            var passwordHasher = new PasswordHasher<User>();
            var refreshToken = new RefreshToken()
            {
                Token = passwordHasher.HashPassword(user, Guid.NewGuid().ToString()),
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
