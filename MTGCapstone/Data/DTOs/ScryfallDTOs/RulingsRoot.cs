namespace MTGCapstone.API.Data.DTOs
{
    public class RulingsRoot
    {
        public string? _object { get; set; }
        public bool has_more { get; set; }
        public List<RulingsDTO>? data { get; set; }
    }
}
