namespace MTGCapstone.API.Data.DTOs
{
    public class PersonalDecksRequest
    {
        private int pageSize = 20;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get => pageSize; }

        public string? Search { get; set; }
        public string? OrderBy { get; set; }
        public string? Format { get; set; }
    }
}
