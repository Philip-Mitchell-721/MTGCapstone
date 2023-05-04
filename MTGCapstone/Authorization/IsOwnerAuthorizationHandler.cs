using Microsoft.AspNetCore.Authorization;
using MTGCapstone.API.Data.Models;
using System.IdentityModel.Tokens.Jwt;

namespace MTGCapstone.API.Authorization
{
    public class IsOwnerAuthorizationHandler : AuthorizationHandler<IsOwnerRequirement, Deck>
    {
        //For resource based policies, make sure that AuthorizationHandler<Requirement, resource> is used.
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsOwnerRequirement requirement, Deck resource)
        {

            System.Security.Claims.Claim? claim = context.User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (claim == null) 
            { 
                return Task.CompletedTask; 
            }

            string userId = claim.Value;

            if (userId == resource.UserId.ToString())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
