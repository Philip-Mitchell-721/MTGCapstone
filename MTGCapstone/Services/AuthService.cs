using AutoMapper;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MTGCapstone.API.Data.DTOs;
using MTGCapstone.API.Data.Models;
using MTGCapstone.API.Data.Models.Identity;
using MTGCapstone.API.Data.Responses;
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
        public async Task<Response<TokenDTO>> RegisterUserAsync(UserRegistrationModel userRegistrationModel)
        {
            try
            {
                //TODO: Register sends email confirmation with token that will redirect to landing page logged in.
                //Check for existing user with username
                User? existingUser = await _userManager.FindByNameAsync(userRegistrationModel.UserName);
                if (existingUser is not null)
                {
                    _logger.LogInformation($"Username {existingUser.UserName} already exists");
                    return new Response<TokenDTO> { Errors = { "User with this Username already exists" } };
                }

                //Create new user
                User user = _mapper.Map<User>(userRegistrationModel);
                IdentityResult result = await _userManager.CreateAsync(user, userRegistrationModel.Password);
                if (!result.Succeeded)
                {
                    return new Response<TokenDTO> { Errors = result.Errors.Select(e => e.Description).ToList() };
                }

                Response<TokenDTO> response = await ConfirmEmailRequestAsync(new ConfirmEmailRequestDTO { UserName = user.UserName});
                if (!response.Success)
                {
                    return response;
                }

                return response;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during RegisterUserAsync");
                return new Response<TokenDTO> { StatusCode = ResponseStatusCodes.Error, Errors = { "Error during RegisterUserAsync" } };
            }
        }
        public async Task<Response<TokenDTO>> LoginAsync(AuthenticationRequestBody authenticationRequestBody)
        {
            User? user = await ValidateUserCredentialsAsync(authenticationRequestBody);

            if (user is null)
            {
                return new Response<TokenDTO> { Errors = { "Invalid user credentials." } };
            }

            return await CreateNewTokensAsync(user);
        }
        public async Task<Response<TokenDTO>> RefreshTokensAsync(RefreshTokenDTO refreshTokenDTO)
        {
            //Validate Access Token Expiration
            JwtSecurityToken token = _jwtSecurityTokenHandler.ReadJwtToken(refreshTokenDTO.AccessToken);
            if (token.ValidTo >= DateTime.UtcNow)
            {
                return new Response<TokenDTO> { Errors = { "Access token hasn't expired yet" } };
            }

            //Validate Refresh Token
            RefreshToken? oldRefreshToken = _capstoneDbContext.RefreshTokens
                .FirstOrDefault(rf => rf.Token == refreshTokenDTO.RefreshToken);

            if (oldRefreshToken is null)
            {
                return new Response<TokenDTO> { Errors = { "Invalid refresh token" } };
            }
            if (DateTime.UtcNow > oldRefreshToken.ExpiredAt)
            {
                return new Response<TokenDTO> { Errors = { "Expired refresh token" } };
            }
            if (oldRefreshToken.JwtId != token.Id)
            {
                return new Response<TokenDTO> { Errors = { "Tokens don't match" } };
            }
            if (oldRefreshToken.Revoked)
            {
                return new Response<TokenDTO> { Errors = { "Revoked refresh token" } };
            }
            if (oldRefreshToken.Used)
            {
                return new Response<TokenDTO> { Errors = { "Previously used refresh token" } };
            }
            User? user = await _capstoneDbContext.Users.FindAsync(oldRefreshToken.UserId);
            if (user is null)
            {
                return new Response<TokenDTO> { Errors = { "User not found" } };
            }

            //Update old Refresh Token
            oldRefreshToken.Used = true;
            await _capstoneDbContext.SaveChangesAsync();


            return await CreateNewTokensAsync(user);
        }
        public async Task<Response<TokenDTO>> RevokeAsync(RefreshTokenToRevokeDTO refreshToken)
        {
            RefreshToken? tokenToRevoke = await _capstoneDbContext.RefreshTokens
                .FirstOrDefaultAsync(rf => rf.Token == refreshToken.RefreshToken);
            if (tokenToRevoke is null)
            {
                return new Response<TokenDTO> { Errors = { "Invalid refresh token" } };
            }
            //Could check to make sure that it's an otherwise valid refresh token, but this still works.

            tokenToRevoke.Revoked = true;
            await _capstoneDbContext.SaveChangesAsync();

            return new Response<TokenDTO> { Success = true };
        }
        public async Task<Response<TokenDTO>> ConfirmEmailRequestAsync(ConfirmEmailRequestDTO confirmEmailRequestDTO)
        {
            User user = await _userManager.FindByNameAsync(confirmEmailRequestDTO.UserName);
            if (user == null)
            {
                return new Response<TokenDTO> { Errors = { "User not found" } };
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string callbackUrl = "https://localhost:7277/authentication/confirm-email";

            Response<TokenDTO> response = await SendTokenInEmailAsync(token, user, callbackUrl);
            if (!response.Success)
            {
                return response;
            }
            return response;
        }
        public async Task<Response<TokenDTO>> ConfirmEmailAsync(ConfirmEmailDTO confirmEmailDTO)
        {
            User user = await _userManager.FindByEmailAsync(confirmEmailDTO.Email);
            if (user == null)
            {
                return new Response<TokenDTO> { Errors = { "User not found" } };
            }


            IdentityResult result = await _userManager.ConfirmEmailAsync(user, confirmEmailDTO.token);

            if (!result.Succeeded)
            {
                return new Response<TokenDTO> { Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            return new Response<TokenDTO> { Success = true };
        }
        public async Task<Response<TokenDTO>> ChangePasswordRequestAsync(ChangePasswordRequestDTO userName)
        {
            User? user = await _userManager.FindByNameAsync(userName.UserName);
            if (user is null)
            {
                return new Response<TokenDTO> { Errors = { "User not found" } };
            }
            
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            string callbackUrl = "https://localhost:7277/authentication/change-password";

            Response<TokenDTO> response = await SendTokenInEmailAsync(resetToken, user, callbackUrl);
            if (!response.Success)
            {
                return response;
            }
            //TODO: This response is being returned like this just so that I can see that the email token is there.
            //remove later.
            return response;

        }
        public async Task<Response<TokenDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            User user = await _userManager.FindByEmailAsync(changePasswordDTO.Email);
            if (user == null)
            {
                return new Response<TokenDTO> { Errors = { "User not found" } };
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(user, changePasswordDTO.token, changePasswordDTO.Password);

            if (!result.Succeeded)
            {
                return new Response<TokenDTO> { Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            return new Response<TokenDTO> { Success = true };
        }


        private async Task<Response<TokenDTO>> SendTokenInEmailAsync(string token, User user, string callbackUrl)
        {
            //TODO: Make this changable with config.
            //TODO: IMPORTANT Change this to whatever url my front end needs to change password.
            string resetTokenUrl = $"{callbackUrl}?email={user.Email}&token={token}";
            string emailBody = $"<a href=\"{resetTokenUrl}\" >Reset Password</a>";

            //TODO: local host is for testing.  Change this to your email server.
            //SmtpSender sender = new SmtpSender(() => new SmtpClient("localhost")
            //{
            //    EnableSsl = false, //For testing
            //    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            //    PickupDirectoryLocation = @"C:\Users\Philip\Documents\EmailTest"
            //});

            //Email.DefaultSender = sender;

            //FluentEmail.Core.Models.SendResponse email = await Email
            //    .From("noreply@MTGCapstone.com")
            //    .To(user.Email, user.UserName)
            //    .Subject("CapstoneMTG Password Reset")
            //    .Body(emailBody, true)
            //    .SendAsync();

            //if (!email.Successful)
            //{
            //    return new Response<TokenDTO> { Errors = email.ErrorMessages.ToList() };
            //}

            //TODO: only returning this right now for testing.  probably only return success=true.
            return new Response<TokenDTO> { Success = true, Value = new TokenDTO { AccessToken = emailBody } };
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
            {
                return null;
            }

            return user;
        }
        private async Task<Response<TokenDTO>> CreateNewTokensAsync(User user)
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
                    expires: DateTime.UtcNow.AddSeconds(600),
                    signingCredentials: signingCredentials
                );


            //Write the accessToken
            string accessToken = _jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);


            string refreshToken = await CreateRefreshTokenAsync(user, jwtSecurityToken.Id);

            return new Response<TokenDTO> { Success = true, Value = new TokenDTO { AccessToken = accessToken, RefreshToken = refreshToken } };
        }
        private async Task<List<Claim>> CreateClaimsAsync(User user)
        {
            //Make the claims
            List<Claim> claimsForToken = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
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
            //TODO: Find each refreshToken for that user and mark them "used" (ASK: or revoke them?)
            //List<RefreshToken> usersRefreshTokens = _capstoneDbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).ToList();
            //foreach (RefreshToken rt in usersRefreshTokens)
            //{
            //    if (rt.ExpiredAt > DateTime.UtcNow)
            //    {
            //        rt.ExpiredAt = DateTime.UtcNow;
            //    }
            //}
            _capstoneDbContext.RefreshTokens.Add(refreshToken);
            await _capstoneDbContext.SaveChangesAsync();

            return refreshToken.Token;
        }
    }
}
