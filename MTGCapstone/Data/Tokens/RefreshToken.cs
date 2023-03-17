namespace MTGCapstone.API.Data.Tokens
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public int UserId { get; set; }
        public bool Used { get; set; }
        public bool Revoked { get; set; }

        public bool IsExpired() => DateTime.UtcNow > ExpiredAt;
    }
}
