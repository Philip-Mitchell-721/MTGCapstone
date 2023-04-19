namespace MTGCapstone.API.Data.Models
{
    public class Ruling
    {
        public int Id { get; set; }
        public string? OracleId { get; set; }
        public string? Source { get; set; }
        public string? PublishedAt { get; set; }
        public string? Comment { get; set; }
    }
}
