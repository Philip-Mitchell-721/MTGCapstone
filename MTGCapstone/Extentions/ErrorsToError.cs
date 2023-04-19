using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace MTGCapstone.API.Extentions
{
    public static class ErrorsToError
    {
        public static string Error(this IdentityResult result)
        {
            return string.Join("\n", result.Errors.Select(e => e.Description));
        }

        public static string Error(this SendResponse result)
        {
            return string.Join("\n", result.ErrorMessages.Select(e => e));
        }

    }
}
