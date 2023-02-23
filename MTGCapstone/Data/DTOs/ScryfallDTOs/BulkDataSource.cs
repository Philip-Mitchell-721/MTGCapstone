namespace MTGCapstone.API.Data.DTOs
{
    public class BulkDataSource
    {
        public string? _object { get; set; }
        public bool has_more { get; set; }
        public List<BulkDataDTO>? data { get; set; }
    }

}
