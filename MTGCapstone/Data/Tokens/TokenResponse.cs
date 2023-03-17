namespace MTGCapstone.API.Data.Tokens
{
    public class TokenResponse
    {
        public bool Success { get; set; } = false;
        public string? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }


    }
}
