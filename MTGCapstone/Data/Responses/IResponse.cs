namespace MTGCapstone.API.Data.Responses
{
    public interface IResponse
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}
