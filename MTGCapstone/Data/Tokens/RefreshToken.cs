namespace MTGCapstone.API.Data.Tokens
{
    public class RefreshToken
    {
        public RefreshToken(string token, long expiration)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Invalid token.");

            if (expiration <= 0)
                throw new ArgumentException("Invalid expiration.");

            Token = token;
            Expiration = expiration;
        }
        public int Id { get; set; }
        public string Token { get; protected set; }
        public long Expiration { get; protected set; }

        public bool IsExpired() => DateTime.UtcNow.Ticks > Expiration;

    }
}
