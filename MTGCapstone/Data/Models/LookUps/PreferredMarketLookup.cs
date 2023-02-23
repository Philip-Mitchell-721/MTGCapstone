namespace MTGCapstone.API.Data.Models
{
    public class PreferredMarketLookup
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public List<User> Users { get; set; } = new List<User>();
    }
}
