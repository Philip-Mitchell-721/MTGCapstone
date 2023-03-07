using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Services
{
    public interface IAccountService
    {
        string CreateJwt(User user);
        Task<User?> ValidateUserCredentialsAsync(string? userName, string? password);

    }
}