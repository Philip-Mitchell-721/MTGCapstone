using AutoMapper;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Tokens;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Extentions;
using NuGet.Packaging.Signing;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;

namespace MTGCapstone.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly CapstoneDbContext _capstoneDbContext;
        private readonly UserManager<User> _userManager;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration,
            CapstoneDbContext capstoneDbContext,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _capstoneDbContext = capstoneDbContext ?? throw new ArgumentNullException(nameof(capstoneDbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        //TODO: Need to add logging to Errors.
        public async Task<TokenResponse> RegisterUserAsync(UserRegistrationModel userRegistrationModel)
        {
            //TODO: Register sends email confirmation with token that will redirect to landing page logged in.
            //Check for existing user with username
            //TODO: add check for user by email
            User? existingUser = await _userManager.FindByNameAsync(userRegistrationModel.UserName);
            if (existingUser is not null)
            {
                _logger.LogInformation($"Username {existingUser.UserName} already exists");
                return new TokenResponse { Error = "User with this Username already exists" };
            }

            //Create new user
            User user = _mapper.Map<User>(userRegistrationModel);
            IdentityResult result = await _userManager.CreateAsync(user, userRegistrationModel.Password);
            if (!result.Succeeded)
            {
                return new TokenResponse { Error = result.Error() };
            }

            TokenResponse response = await ConfirmEmailRequestAsync(new ConfirmEmailRequestDTO { UserName = user.UserName});
            if (!response.Success)
            {
                return response;
            }

            return new TokenResponse { Success = true };
        }
        public async Task<TokenResponse> LoginAsync(AuthenticationRequestBody authenticationRequestBody)
        {
            User? user = await ValidateUserCredentialsAsync(authenticationRequestBody);

            if (user is null)
                return new TokenResponse { Error = "Invalid user credentials." };

            return await CreateNewTokensAsync(user);
        }
        public async Task<TokenResponse> RefreshTokensAsync(RefreshTokenDTO refreshTokenDTO)
        {
            //Validate Access Token Expiration
            JwtSecurityToken token = _jwtSecurityTokenHandler.ReadJwtToken(refreshTokenDTO.AccessToken);
            if (token.ValidTo >= DateTime.UtcNow)
            {
                return new TokenResponse { Error = "Access token hasn't expired yet" };

            }

            //Validate Refresh Token
            RefreshToken? oldRefreshToken = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refreshTokenDTO.RefreshToken);

            if (oldRefreshToken is null)
            {
                return new TokenResponse { Error = "Invalid refresh token" };
            }
            if (DateTime.UtcNow > oldRefreshToken.ExpiredAt)
            {
                return new TokenResponse { Error = "Expired refresh token" };
            }
            if (oldRefreshToken.JwtId != token.Id)
            {
                return new TokenResponse { Error = "Tokens don't match" };
            }
            if (oldRefreshToken.Used)
            {
                return new TokenResponse { Error = "Previously used refresh token" };
            }
            if (oldRefreshToken.Revoked)
            {
                return new TokenResponse { Error = "Revoked refresh token" };
            }
            User? user = await _capstoneDbContext.Users.FindAsync(oldRefreshToken.UserId);
            if (user is null)
            {
                return new TokenResponse { Error = "User not found" };
            }

            //TODO: ADD ALL THE BRACKETS (╯°□°)╯︵ ┻━┻
            //Update old Refresh Token
            oldRefreshToken.Used = true;
            await _capstoneDbContext.SaveChangesAsync();


            return await CreateNewTokensAsync(user);
        }
        public async Task<TokenResponse> RevokeAsync(RefreshTokenToRevokeDTO refreshToken)
        {
            RefreshToken? tokenToRevoke = await _capstoneDbContext.RefreshTokens
                .FirstOrDefaultAsync(rf => rf.Token == refreshToken.RefreshToken);
            if (tokenToRevoke is null)
                return new TokenResponse { Error = "Invalid refresh token" };
            //Could check to make sure that it's an otherwise valid refresh token, but this still works.

            tokenToRevoke.Revoked = true;
            await _capstoneDbContext.SaveChangesAsync();

            return new TokenResponse { Success = true };
        }
        public async Task<TokenResponse> ConfirmEmailRequestAsync(ConfirmEmailRequestDTO confirmEmailRequestDTO)
        {
            User user = await _userManager.FindByNameAsync(confirmEmailRequestDTO.UserName);
            if (user == null)
                return new TokenResponse { Error = "User not found" };

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string callbackUrl = "https://localhost:7277/authentication/confirm-email";

            TokenResponse response = await SendTokenInEmailAsync(token, user, callbackUrl);
            if (!response.Success)
            {
                return response;
            }
            return new TokenResponse { Success = true };
        }
        public async Task<TokenResponse> ConfirmEmailAsync(ConfirmEmailDTO confirmEmailDTO)
        {
            User user = await _userManager.FindByEmailAsync(confirmEmailDTO.Email);
            if (user == null)
                return new TokenResponse { Error = "User not found" };


            IdentityResult result = await _userManager.ConfirmEmailAsync(user, confirmEmailDTO.token);

            if (!result.Succeeded)
                return new TokenResponse { Error = result.Error() };

            return new TokenResponse { Success = true };
        }
        public async Task<TokenResponse> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName)
        {
            User? user = await _userManager.FindByNameAsync(userName.UserName);
            if (user is null)
                return new TokenResponse { Error = "User not found" };
            
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            string callbackUrl = "https://localhost:7277/authentication/change-password";

            TokenResponse response = await SendTokenInEmailAsync(resetToken, user, callbackUrl);
            if (!response.Success)
            {
                return response;
            }
            return new TokenResponse { Success = true };

        }
        public async Task<TokenResponse> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            User user = await _userManager.FindByEmailAsync(changePasswordDTO.Email);
            if (user == null)
                return new TokenResponse { Error = "User not found" };

            IdentityResult result = await _userManager.ResetPasswordAsync(user, changePasswordDTO.token, changePasswordDTO.Password);

            if (!result.Succeeded)
                return new TokenResponse { Error = result.Error() };

            return new TokenResponse { Success = true };
        }


        private async Task<TokenResponse> SendTokenInEmailAsync(string token, User user, string callbackUrl)
        {
            //TODO: Make this changable with config.
            //TODO: IMPORTANT Change this to whatever url my front end needs to change password.
            string resetTokenUrl = $"{callbackUrl}?email={user.Email}&token={token}";
            string emailBody = $"<a href=\"{resetTokenUrl}\" >Reset Password</a>";

            //TODO: local host is for testing.  Change this to your email server.
            SmtpSender sender = new SmtpSender(() => new SmtpClient("localhost")
            {
                EnableSsl = false, //For testing
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = @"C:\Users\Philip\Documents\EmailTest"
            });

            Email.DefaultSender = sender;

            FluentEmail.Core.Models.SendResponse email = await Email
                .From("noreply@MTGCapstone.com")
                .To(user.Email, user.UserName)
                .Subject("CapstoneMTG Password Reset")
                .Body(emailBody, true)
                .SendAsync();

            if (!email.Successful)
            {
                return new TokenResponse { Error = email.Error() };
            }

            //TODO: only returning this right now for testing.  probably only return success=true.
            return new TokenResponse { Success = true, AccessToken = emailBody };
        }
        private async Task<User?> ValidateUserCredentialsAsync(AuthenticationRequestBody authenticationRequestBody)
        {
            User user = await _userManager.FindByNameAsync(authenticationRequestBody.UserName);

            if (user == null)
            {
                //_logger.log()
                return null;
            }

            if (!await _userManager.CheckPasswordAsync(user, authenticationRequestBody.Password))
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
            List<Claim> claimsForToken = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
            claimsForToken.AddRange(userClaims);

            //Add roles to the claims
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            foreach (string? userRole in userRoles)
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
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));

            SigningCredentials signingCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256);
            return signingCredentials;
        }
        private async Task<string> CreateRefreshTokenAsync(User user, string jwtId)
        {
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            RefreshToken refreshToken = new RefreshToken()
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
