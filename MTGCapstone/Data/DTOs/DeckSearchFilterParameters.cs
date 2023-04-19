namespace MTGCapstone.API.Data.DTOs
{
    public class GetDecksRequest
    {
        const int maxPageSize = 20;
        private int pageSize = 20;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => pageSize;
            set => pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string? Search { get; set; }
        public string? OrderBy { get; set; }
        public string? UserName { get; set; }
        public string? Format { get; set; }
        public string? Commander { get; set; }
    }
}
