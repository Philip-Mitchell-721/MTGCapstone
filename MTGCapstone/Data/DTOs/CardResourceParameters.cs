namespace MTGCapstone.API.Data.DTOs
{
    public class CardResourceParameters
    {
        const int maxPageSize = 50;
        private int pageSize = 20;
        public int PageNumber { get; set; } = 1;
        public int PageSize 
        { 
            get => pageSize; 
            set => pageSize = (value > maxPageSize) ? maxPageSize : value; 
        }

        public string? Search { get; set; }
        public string OrderBy { get; set; } = "EdhrecRank";

        public string? Name { get; set; }
        public string? Type { get; set; }
        public string[]? Colors { get; set; } //TODO: add colors to filters
        public string? Language { get; set; } = "en";
    }
}
