namespace MTGCapstone.API.Data.Responses
{
    public class Response<T>
    {
        public ResponseStatusCodes? StatusCode { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public T? Value { get; set; }
        public bool Success { get; set; }
    }

    public class Response
    {
        public ResponseStatusCodes? StatusCode { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool Success { get; set; }
    }

}
