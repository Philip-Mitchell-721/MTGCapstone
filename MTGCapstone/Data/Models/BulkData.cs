namespace MTGCapstone.API.Data.Models
{
    public class BulkData
    {
        public int Id { get; set; }
        public string? ScryfallId { get; set; }
        public string? Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Uri { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int CompressedSize { get; set; }
        public string? DownloadUri { get; set; }
        public string? ContentType { get; set; }
        public string? ContentEncoding { get; set; }
    }
}
