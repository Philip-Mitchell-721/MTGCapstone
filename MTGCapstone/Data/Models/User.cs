using Microsoft.AspNetCore.Identity;
using System.Net.Sockets;

namespace MTGCapstone.API.Data.Models
{
    public class User : IdentityUser<int>
    {

        //TODO: Migrate/Update database.  IdentityUser added.
        //public string? Id { get; set; }
        //public string? UserName { get; set; }
        //public string? Email { get; set; }
        public string? Bio { get; set; }
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Deck> Decks { get; set; } = new List<Deck>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Packet> Packets { get; set; } = new List<Packet>();

        public int? PreferredPriceUnitLookupId { get; set; }
        public PreferredPriceUnitLookup? PreferredPriceUnitLookup { get; set; }
        public int? PreferredMarketLookupId { get; set; }
        public PreferredMarketLookup? PreferredMarketLookup { get; set; }

    }

}
