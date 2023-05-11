namespace MTGCapstone.API.Data.DTOs
{
    public class PersonalDecksRequest
    {
        public int PageNumber { get; set; } = 1;

        public string? Search { get; set; }
        //public string? OrderBy { get; set; } Add this back in later.
        
        public string? Format { get; set; }
    }
}
