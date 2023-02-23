using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Data.DTOs
{
    public class ScryfallCardRootobject
    {
        public string? _object { get; set; }
        public int total_cards { get; set; }
        public bool has_more { get; set; }
        public string? next_page { get; set; }
        public List<ScryfallCard>? data { get; set; }
    }
}
