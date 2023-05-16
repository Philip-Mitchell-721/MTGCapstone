using Microsoft.CodeAnalysis.CSharp.Syntax;
using MTGCapstone.API.Data.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MTGCapstone.API.Extentions
{
    public static class UserExtensions
    {
        public static int? Id(this ClaimsPrincipal claimsPrincipal)
        {
            return int.TryParse(claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Sub), out int id) ? id : null;
        }
    }
}
