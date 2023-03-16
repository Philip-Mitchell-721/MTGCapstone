using Microsoft.IdentityModel.JsonWebTokens;

namespace MTGCapstone.API.Data.Tokens
{
    public class AccessToken
    {

        public AccessToken(string token, long expiration, RefreshToken refreshToken)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Invalid token.");

            if (expiration <= 0)
                throw new ArgumentException("Invalid expiration.");

            Token = token;
            Expiration = expiration;
            RefreshToken = refreshToken;
        }

        public string Token { get; protected set; }
        public long Expiration { get; protected set; }
        public RefreshToken RefreshToken { get; private set; }

        public bool IsExpired() => DateTime.UtcNow.Ticks > Expiration;

    }

}
