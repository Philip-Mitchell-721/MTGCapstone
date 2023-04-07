using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MTGCapstone.API.Extentions
{
    public static class UserExtensions
    {
        public static int Id(this ClaimsPrincipal claimsPrincipal)
        {
            return int.Parse(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        }
    }
}
