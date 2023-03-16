namespace MTGCapstone.API.Data.Tokens
{
    public class TokenResponse
    {
        public bool Success;
        public string Message;
        public string? Token;
        public string? RefreshToken { get; set; }

        public TokenResponse(bool success, string message, string? token, string? refreshToken)
        {
            Success = success;
            Message = message;
            Token = token;
        }
    }
}
