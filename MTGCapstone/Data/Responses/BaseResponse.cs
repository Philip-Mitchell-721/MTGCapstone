namespace MTGCapstone.API.Data.Responses
{
    public class BaseResponse : IResponse
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

    }
}
